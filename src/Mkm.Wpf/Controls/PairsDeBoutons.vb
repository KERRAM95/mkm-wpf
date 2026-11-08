Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le nom qu'un lecteur d'écran donne à un bouton de mkm : son nom d'automatisation, sinon son libellé, sinon son
    ''' info-bulle. Un bouton d'icône seule (une icône et une info-bulle, comme « Actualiser ») n'avait pas de nom.
    ''' </summary>
    Friend Module NomDeBouton

        ''' <summary>L'info-bulle d'un élément quand c'est un texte, sinon une chaîne vide.</summary>
        Friend Function InfoBulle(element As UIElement) As String
            Return If(TryCast(TryCast(element, FrameworkElement)?.ToolTip, String), String.Empty)
        End Function

    End Module

    ''' <summary>Le pair d'automatisation de <see cref="Button"/> : son info-bulle le nomme quand il n'a pas de libellé.</summary>
    Friend NotInheritable Class PairDeBouton
        Inherits ButtonAutomationPeer

        Friend Sub New(proprietaire As Button)
            MyBase.New(proprietaire)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function GetNameCore() As String
            Dim nom = MyBase.GetNameCore()
            Return If(String.IsNullOrEmpty(nom), InfoBulle(Owner), nom)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetClassNameCore() As String
            Return Owner.GetType().Name
        End Function

    End Class

    ''' <summary>
    ''' Le pair d'automatisation de <see cref="ToggleButton"/> : son libellé d'état (OnLabel, OffLabel) le nomme, sinon
    ''' son contenu, sinon son info-bulle.
    ''' </summary>
    Friend NotInheritable Class PairDeBascule
        Inherits ToggleButtonAutomationPeer

        Friend Sub New(proprietaire As ToggleButton)
            MyBase.New(proprietaire)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function GetNameCore() As String
            Dim nom = AutomationProperties.GetName(Owner)
            If Not String.IsNullOrEmpty(nom) Then Return nom
            Dim libelle = TryCast(DirectCast(Owner, ToggleButton).CurrentLabel, String)
            If Not String.IsNullOrEmpty(libelle) Then Return libelle
            nom = MyBase.GetNameCore()
            Return If(String.IsNullOrEmpty(nom), InfoBulle(Owner), nom)
        End Function

        ''' <inheritdoc/>
        Protected Overrides Function GetClassNameCore() As String
            Return Owner.GetType().Name
        End Function

    End Class

End Namespace
