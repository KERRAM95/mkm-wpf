Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La zone de défilement de mkm (ScrollPanel) : son contenu défile dans les deux sens, ses barres fines de 9 px
    ''' posées sur lui, qui ne paraissent qu'au survol : le contenu garde toute la place.
    ''' <code>
    ''' &lt;mkm:ScrollPanel Height="200"&gt;
    '''     &lt;TextBlock Text="…" TextWrapping="Wrap" /&gt;
    ''' &lt;/mkm:ScrollPanel&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' C'est un <see cref="ScrollViewer"/> : la molette, le clavier et l'accessibilité de WPF. Les barres de défilement
    ''' de toute l'application (celles des listes, des tableaux, des pages) ont déjà l'apparence de celles de mkm :
    ''' <see cref="MkmResources"/> les habille ; ScrollPanel y ajoute des barres posées sur le contenu, au survol.
    ''' </remarks>
    Public Class ScrollPanel
        Inherits ScrollViewer

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(ScrollPanel), New FrameworkPropertyMetadata(GetType(ScrollPanel)))
            HorizontalScrollBarVisibilityProperty.OverrideMetadata(GetType(ScrollPanel), New FrameworkPropertyMetadata(ScrollBarVisibility.Auto))
        End Sub

    End Class

End Namespace
