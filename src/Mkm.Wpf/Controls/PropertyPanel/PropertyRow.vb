Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Une propriété d'une <see cref="PropertyPanel"/> : son libellé gris, et dessous (ou devant, en
    ''' <see cref="PropertyPanelLayout.Inline"/>) sa valeur (<see cref="Value"/>) ; en modification, son éditeur
    ''' (le contenu de la ligne : un InputText, un Select…), la marque d'une propriété obligatoire
    ''' (<see cref="Required"/>) et son message (<see cref="Message"/>). Une note (<see cref="Note"/>) se lit sous la
    ''' valeur.
    ''' </summary>
    Public Class PropertyRow
        Inherits ContentControl

        ''' <summary>Le libellé.</summary>
        Public Shared ReadOnly LabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Label), GetType(String), GetType(PropertyRow), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>La valeur, en lecture.</summary>
        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Value), GetType(Object), GetType(PropertyRow), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>L'image ou l'icône devant la valeur, en lecture (18 px au plus).</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(PropertyRow), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai si la propriété est obligatoire : une marque rouge suit son libellé, en modification.</summary>
        Public Shared ReadOnly RequiredProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Required), GetType(Boolean), GetType(PropertyRow), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai si la propriété reste en lecture quand la fiche est en modification.</summary>
        Public Shared ReadOnly ReadOnlyProperty As DependencyProperty = DependencyProperty.Register(
            NameOf([ReadOnly]), GetType(Boolean), GetType(PropertyRow), New FrameworkPropertyMetadata(False, AddressOf OnEtatChanged))

        ''' <summary>Un petit texte sous la valeur.</summary>
        Public Shared ReadOnly NoteProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Note), GetType(String), GetType(PropertyRow), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le message sous l'éditeur, en modification (une erreur par défaut, voir <see cref="MessageSeverity"/>).</summary>
        Public Shared ReadOnly MessageProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Message), GetType(String), GetType(PropertyRow), New FrameworkPropertyMetadata(Nothing, AddressOf OnEtatChanged))

        ''' <summary>La gravité du message : <c>Error</c> (par défaut), <c>Warn</c> ou <c>Info</c> ; le libellé en prend la couleur.</summary>
        Public Shared ReadOnly MessageSeverityProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(MessageSeverity), GetType(MessageSeverity), GetType(PropertyRow), New FrameworkPropertyMetadata(MessageSeverity.Error))

        ''' <summary>Si la fiche est en modification (héritée de <see cref="PropertyPanel"/>).</summary>
        Public Shared ReadOnly EditingProperty As DependencyProperty = PropertyPanel.EditingProperty.AddOwner(
            GetType(PropertyRow), New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.Inherits, AddressOf OnEtatChanged))

        ''' <summary>Où se place le libellé (héritée de <see cref="PropertyPanel"/>).</summary>
        Public Shared ReadOnly LayoutProperty As DependencyProperty = PropertyPanel.LayoutProperty.AddOwner(GetType(PropertyRow))

        Private Shared ReadOnly HasMessagePropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(HasMessage), GetType(Boolean), GetType(PropertyRow), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand la ligne a un message.</summary>
        Public Shared ReadOnly HasMessageProperty As DependencyProperty = HasMessagePropertyKey.DependencyProperty

        Private Shared ReadOnly IsEditorShownPropertyKey As DependencyPropertyKey = DependencyProperty.RegisterReadOnly(
            NameOf(IsEditorShown), GetType(Boolean), GetType(PropertyRow), New FrameworkPropertyMetadata(False))
        ''' <summary>Vrai quand la ligne montre son éditeur : en modification, s'il y en a un et qu'elle n'est pas en lecture seule.</summary>
        Public Shared ReadOnly IsEditorShownProperty As DependencyProperty = IsEditorShownPropertyKey.DependencyProperty

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(PropertyRow), New FrameworkPropertyMetadata(GetType(PropertyRow)))
            FocusableProperty.OverrideMetadata(GetType(PropertyRow), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>Le libellé.</summary>
        Public Property Label As String
            Get
                Return CStr(GetValue(LabelProperty))
            End Get
            Set(value As String)
                SetValue(LabelProperty, value)
            End Set
        End Property

        ''' <summary>La valeur, en lecture.</summary>
        Public Property Value As Object
            Get
                Return GetValue(ValueProperty)
            End Get
            Set(value As Object)
                SetValue(ValueProperty, value)
            End Set
        End Property

        ''' <summary>L'image ou l'icône devant la valeur, en lecture.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si la propriété est obligatoire.</summary>
        Public Property Required As Boolean
            Get
                Return CBool(GetValue(RequiredProperty))
            End Get
            Set(value As Boolean)
                SetValue(RequiredProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si la propriété reste en lecture quand la fiche est en modification.</summary>
        Public Property [ReadOnly] As Boolean
            Get
                Return CBool(GetValue(ReadOnlyProperty))
            End Get
            Set(value As Boolean)
                SetValue(ReadOnlyProperty, value)
            End Set
        End Property

        ''' <summary>Un petit texte sous la valeur.</summary>
        Public Property Note As String
            Get
                Return CStr(GetValue(NoteProperty))
            End Get
            Set(value As String)
                SetValue(NoteProperty, value)
            End Set
        End Property

        ''' <summary>Le message sous l'éditeur, en modification.</summary>
        Public Property Message As String
            Get
                Return CStr(GetValue(MessageProperty))
            End Get
            Set(value As String)
                SetValue(MessageProperty, value)
            End Set
        End Property

        ''' <summary>La gravité du message.</summary>
        Public Property MessageSeverity As MessageSeverity
            Get
                Return DirectCast(GetValue(MessageSeverityProperty), MessageSeverity)
            End Get
            Set(value As MessageSeverity)
                SetValue(MessageSeverityProperty, value)
            End Set
        End Property

        ''' <summary>Si la fiche est en modification.</summary>
        Public ReadOnly Property Editing As Boolean
            Get
                Return CBool(GetValue(EditingProperty))
            End Get
        End Property

        ''' <summary>Où se place le libellé.</summary>
        Public ReadOnly Property Layout As PropertyPanelLayout
            Get
                Return DirectCast(GetValue(LayoutProperty), PropertyPanelLayout)
            End Get
        End Property

        ''' <summary>Vrai quand la ligne montre son éditeur.</summary>
        Public ReadOnly Property IsEditorShown As Boolean
            Get
                Return CBool(GetValue(IsEditorShownProperty))
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides Sub OnContentChanged(oldContent As Object, newContent As Object)
            MyBase.OnContentChanged(oldContent, newContent)
            MettreAJour()
        End Sub

        Private Shared Sub OnEtatChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, PropertyRow).MettreAJour()
        End Sub

        Private Sub MettreAJour()
            SetValue(IsEditorShownPropertyKey, Editing AndAlso Not [ReadOnly] AndAlso Content IsNot Nothing)
            SetValue(HasMessagePropertyKey, Not String.IsNullOrEmpty(Message))
        End Sub

        ''' <summary>Vrai quand la ligne a un message.</summary>
        Public ReadOnly Property HasMessage As Boolean
            Get
                Return CBool(GetValue(HasMessageProperty))
            End Get
        End Property

    End Class

End Namespace
