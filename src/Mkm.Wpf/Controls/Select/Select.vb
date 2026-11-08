Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La liste déroulante de mkm (<c>Select</c>), au rendu de 3DEXPERIENCE : un champ de 24 px et, à sa
    ''' droite, un bouton gris de 22 px qui porte la flèche ; la liste s'ouvre 4 px dessous, ses choix de
    ''' 22 px séparés d'un trait, le choix actuel cerné. <see cref="Placeholder"/> et <see cref="Invalid"/>
    ''' comme <see cref="InputText"/>.
    ''' </summary>
    ''' <remarks>
    ''' C'est un <see cref="ComboBox"/> : <c>IsEditable</c> (une valeur hors de la liste), la recherche au
    ''' clavier, F4 et Alt+Bas, l'accessibilité sont ceux de WPF. La liste est virtualisée : seuls les choix
    ''' visibles existent, si bien qu'une liste de milliers de valeurs s'ouvre aussitôt. « Select » est un mot
    ''' réservé de VB : la classe s'écrit <c>[Select]</c> en VB, <c>mkm:Select</c> en XAML.
    ''' </remarks>
    Public Class [Select]
        Inherits ComboBox

        ''' <summary>Sans libellé, son texte indicatif le nomme pour les lecteurs d'écran.</summary>
        Protected Overrides Function OnCreateAutomationPeer() As System.Windows.Automation.Peers.AutomationPeer
            Return New PairDeListe(Me)
        End Function

        ''' <summary>Le texte d'aide, montré quand rien n'est choisi.</summary>
        Public Shared ReadOnly PlaceholderProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Placeholder), GetType(String), GetType([Select]), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Vrai quand la valeur est fausse : le champ prend la couleur d'erreur.</summary>
        Public Shared ReadOnly InvalidProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Invalid), GetType(Boolean), GetType([Select]), New FrameworkPropertyMetadata(False))

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType([Select]), New FrameworkPropertyMetadata(GetType([Select])))
        End Sub

        ''' <summary>Le texte d'aide.</summary>
        Public Property Placeholder As String
            Get
                Return CStr(GetValue(PlaceholderProperty))
            End Get
            Set(value As String)
                SetValue(PlaceholderProperty, value)
            End Set
        End Property

        ''' <summary>Vrai quand la valeur est fausse.</summary>
        Public Property Invalid As Boolean
            Get
                Return CBool(GetValue(InvalidProperty))
            End Get
            Set(value As Boolean)
                SetValue(InvalidProperty, value)
            End Set
        End Property

    End Class

End Namespace
