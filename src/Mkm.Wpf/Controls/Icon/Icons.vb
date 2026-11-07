Imports System.Windows.Media

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Les icônes de mkm-wpf : celles de mkm, nommées par leur sens (<c>Icons.Add</c>,
    ''' <c>Icons.Close</c>), dessinées par Tabler Icons sur sa grille de 24. Chacune est une géométrie
    ''' figée, que <see cref="Icon"/> trace à la couleur du texte : <c>{x:Static mkm:Icons.Add}</c>.
    ''' </summary>
    ''' <remarks>Les géométries sont dans <c>Icons.g.vb</c>, produit par <c>outils/icones/importer-tabler.mjs</c>.</remarks>
    Partial Public NotInheritable Class Icons

        Private Sub New()
        End Sub

        Friend Shared Function Dessin(donnees As String) As Geometry
            Dim geometrie = Geometry.Parse(donnees)
            geometrie.Freeze()
            Return geometrie
        End Function

    End Class

End Namespace
