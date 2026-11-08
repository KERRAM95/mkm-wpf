Imports System
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le nom d'un champ pour les lecteurs d'écran : son nom d'automatisation ou son libellé (<c>LabeledBy</c>), sinon son
    ''' texte indicatif (<c>Placeholder</c>), comme le fait le web pour un champ sans libellé.
    ''' </summary>
    Friend Module NomDeChamp

        Friend Function Nommer(nom As String, indication As String) As String
            Return If(String.IsNullOrEmpty(nom), If(indication, String.Empty), nom)
        End Function

        ''' <summary>
        ''' L'état d'un champ refusé (sa propriété <c>Invalid</c>), dit aux lecteurs d'écran comme le <c>aria-invalid</c> de
        ''' mkm ; sinon l'état posé par l'application.
        ''' </summary>
        Friend Function EtatDuChamp(champ As UIElement, etat As String) As String
            If Not String.IsNullOrEmpty(etat) Then Return etat
            Dim refuse = System.ComponentModel.DependencyPropertyDescriptor.FromName("Invalid", champ.GetType(), champ.GetType())
            If refuse IsNot Nothing AndAlso CBool(refuse.GetValue(champ)) Then Return Localization.Localizer.Current.T("aria.invalid")
            Return String.Empty
        End Function

    End Module

    ''' <summary>Le pair d'automatisation d'<see cref="InputText"/> et de <see cref="Textarea"/>.</summary>
    Friend NotInheritable Class PairDeTexte
        Inherits TextBoxAutomationPeer

        Private ReadOnly _indication As Func(Of String)

        Friend Sub New(proprietaire As TextBox, indication As Func(Of String))
            MyBase.New(proprietaire)
            _indication = indication
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function GetNameCore() As String
            Return Nommer(MyBase.GetNameCore(), _indication())
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetItemStatusCore() As String
            Return EtatDuChamp(Owner, MyBase.GetItemStatusCore())
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetClassNameCore() As String
            Return Owner.GetType().Name
        End Function

    End Class

    ''' <summary>Le pair d'automatisation de <see cref="[Select]"/>.</summary>
    Friend NotInheritable Class PairDeListe
        Inherits ComboBoxAutomationPeer

        Friend Sub New(proprietaire As [Select])
            MyBase.New(proprietaire)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function GetNameCore() As String
            Return Nommer(MyBase.GetNameCore(), DirectCast(Owner, [Select]).Placeholder)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetItemStatusCore() As String
            Return EtatDuChamp(Owner, MyBase.GetItemStatusCore())
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetClassNameCore() As String
            Return NameOf([Select])
        End Function

    End Class

    ''' <summary>
    ''' Le pair d'automatisation d'<see cref="InputNumber"/> : une toupie (Spinner), comme le <c>spinbutton</c> de mkm, qui
    ''' dit sa valeur et ses bornes, et que le lecteur d'écran change ; son champ et ses boutons dedans.
    ''' </summary>
    Friend NotInheritable Class PairDeNombre
        Inherits PairDeConteneur
        Implements IRangeValueProvider

        Friend Sub New(proprietaire As InputNumber)
            MyBase.New(proprietaire, AutomationControlType.Spinner, Function() proprietaire.Placeholder)
        End Sub

        Private ReadOnly Property Nombre As InputNumber
            Get
                Return DirectCast(Owner, InputNumber)
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Function GetPattern(patternInterface As PatternInterface) As Object
            If patternInterface = PatternInterface.RangeValue Then Return Me
            Return MyBase.GetPattern(patternInterface)
        End Function

        ''' <summary>La valeur ; 0 quand le champ est vide.</summary>
        Public ReadOnly Property Value As Double Implements IRangeValueProvider.Value
            Get
                Return Nombre.Value.GetValueOrDefault()
            End Get
        End Property

        ''' <inheritdoc/>
        Public ReadOnly Property Minimum As Double Implements IRangeValueProvider.Minimum
            Get
                Return Nombre.Minimum
            End Get
        End Property

        ''' <inheritdoc/>
        Public ReadOnly Property Maximum As Double Implements IRangeValueProvider.Maximum
            Get
                Return Nombre.Maximum
            End Get
        End Property

        ''' <summary>Le pas des boutons et des flèches.</summary>
        Public ReadOnly Property SmallChange As Double Implements IRangeValueProvider.SmallChange
            Get
                Return Nombre.Step
            End Get
        End Property

        ''' <summary>Le pas aussi : un InputNumber n'a pas de grand pas.</summary>
        Public ReadOnly Property LargeChange As Double Implements IRangeValueProvider.LargeChange
            Get
                Return Nombre.Step
            End Get
        End Property

        ''' <inheritdoc/>
        Public ReadOnly Property IsReadOnly As Boolean Implements IRangeValueProvider.IsReadOnly
            Get
                Return Nombre.IsReadOnly OrElse Not Nombre.IsEnabled
            End Get
        End Property

        ''' <summary>Pose une valeur, ramenée entre les bornes.</summary>
        Public Sub SetValue(value As Double) Implements IRangeValueProvider.SetValue
            If Not IsEnabled() Then Throw New ElementNotEnabledException()
            If Nombre.IsReadOnly Then Throw New InvalidOperationException()
            Nombre.Value = Math.Max(Nombre.Minimum, Math.Min(Nombre.Maximum, value))
        End Sub

    End Class

End Namespace
