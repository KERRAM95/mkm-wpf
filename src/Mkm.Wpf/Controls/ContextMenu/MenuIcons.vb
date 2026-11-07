Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' La colonne d'icône d'un menu de mkm : un menu dont un élément a une icône (ou se coche) garde pour tous une
    ''' colonne de 44 px où commence le libellé ; un menu sans aucune icône n'en a pas, le libellé commence à 14 px.
    ''' Posé sur chaque menu à son ouverture (et sur chaque sous-menu), hérité par ses éléments.
    ''' </summary>
    Public NotInheritable Class MenuIcons

        ''' <summary>Vrai si les éléments du menu gardent la colonne d'icône.</summary>
        Public Shared ReadOnly HasIconsProperty As DependencyProperty = DependencyProperty.RegisterAttached(
            "HasIcons", GetType(Boolean), GetType(MenuIcons),
            New FrameworkPropertyMetadata(True, FrameworkPropertyMetadataOptions.Inherits))

        Private Shared _enregistre As Boolean

        Private Sub New()
        End Sub

        ''' <summary>Si les éléments sous <paramref name="element"/> gardent la colonne d'icône.</summary>
        Public Shared Function GetHasIcons(element As DependencyObject) As Boolean
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            Return CBool(element.GetValue(HasIconsProperty))
        End Function

        ''' <summary>Pose la colonne d'icône des éléments sous <paramref name="element"/>.</summary>
        Public Shared Sub SetHasIcons(element As DependencyObject, value As Boolean)
            If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
            element.SetValue(HasIconsProperty, value)
        End Sub

        ' Une fois par application (MkmResources) : chaque menu contextuel et chaque sous-menu, à son ouverture.
        Friend Shared Sub Enregistrer()
            If _enregistre Then Return
            _enregistre = True
            EventManager.RegisterClassHandler(GetType(ContextMenu), ContextMenu.OpenedEvent, New RoutedEventHandler(AddressOf MenuOuvert))
            EventManager.RegisterClassHandler(GetType(MenuItem), MenuItem.SubmenuOpenedEvent, New RoutedEventHandler(AddressOf SousMenuOuvert))
        End Sub

        Private Shared Sub MenuOuvert(sender As Object, e As RoutedEventArgs)
            Dim menu = DirectCast(sender, ContextMenu)
            SetHasIcons(menu, OntDesIcones(menu))
        End Sub

        Private Shared Sub SousMenuOuvert(sender As Object, e As RoutedEventArgs)
            If e.OriginalSource IsNot sender Then Return
            Dim element = DirectCast(sender, MenuItem)
            Dim cadre = TryCast(element.Template?.FindName("SousMenu", element), FrameworkElement)
            If cadre IsNot Nothing Then SetHasIcons(cadre, OntDesIcones(element))
        End Sub

        Private Shared Function OntDesIcones(liste As ItemsControl) As Boolean
            Return Enumerable.Range(0, liste.Items.Count).
                Select(Function(i) If(TryCast(liste.Items(i), MenuItem), TryCast(liste.ItemContainerGenerator.ContainerFromIndex(i), MenuItem))).
                Any(Function(m) m IsNot Nothing AndAlso (m.Icon IsNot Nothing OrElse m.IsCheckable))
        End Function

    End Class

End Namespace
