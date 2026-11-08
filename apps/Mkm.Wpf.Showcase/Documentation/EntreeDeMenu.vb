Namespace Mkm.Wpf.Showcase.Documentation

    ''' <summary>Une entrée du menu de gauche : une page de la documentation, rangée dans sa catégorie.</summary>
    Public NotInheritable Class EntreeDeMenu

        ''' <summary>Le titre, tel qu'il paraît dans le menu.</summary>
        Public Property Titre As String

        ''' <summary>La catégorie (le groupe du menu).</summary>
        Public Property Categorie As String

        ''' <summary>L'identifiant de la page.</summary>
        Public Property Id As String

        ''' <summary>L'identifiant d'automatisation de l'entrée (« nav-button »), pour les tests d'interaction.</summary>
        Public Property Automatisation As String

        ''' <summary>Vrai pour une page de la documentation ; faux pour une page d'origine de la vitrine.</summary>
        Public Property EstDocumentee As Boolean

        ''' <summary>Le titre : ce que lit un lecteur d'écran.</summary>
        Public Overrides Function ToString() As String
            Return Titre
        End Function

    End Class

End Namespace
