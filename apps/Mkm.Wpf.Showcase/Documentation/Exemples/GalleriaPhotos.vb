Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports Mkm.Wpf.Controls

Namespace Mkm.Wpf.Showcase.Documentation.Exemples

    ''' <summary>Une photo de la galerie : son image et son titre.</summary>
    Public NotInheritable Class PhotoDeGalerie
        Public Property Source As ImageSource
        Public Property Titre As String

        ''' <summary>Le nom de la vignette pour les lecteurs d'écran.</summary>
        Public Overrides Function ToString() As String
            Return Titre
        End Function
    End Class

    ''' <summary>Les galeries de l'exemple reçoivent leurs photos (celles d'ui-kit).</summary>
    Friend NotInheritable Class GalleriaPhotos

        Friend Shared Sub Preparer(exemple As FrameworkElement)
            For Each nom In {"Photos", "Diaporama"}
                Dim galerie = TryCast(exemple.FindName(nom), Galleria)
                If galerie IsNot Nothing Then Remplir(galerie)
            Next
        End Sub

#Region "Exemple"
        Private Shared Sub Remplir(galerie As Galleria)
            Dim photo = Function(fichier As String, titre As String) New PhotoDeGalerie With {
                .Source = New BitmapImage(New Uri($"pack://application:,,,/Images/{fichier}", UriKind.Absolute)), .Titre = titre}
            galerie.ItemsSource = New ObservableCollection(Of PhotoDeGalerie) From {
                photo("island1.jpg", "L'île, au crépuscule"),
                photo("island2.jpg", "L'île, au matin"),
                photo("galleria10.jpg", "Le canal, la nuit")}
        End Sub
#End Region

    End Class

End Namespace
