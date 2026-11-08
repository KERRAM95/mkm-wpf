Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Data
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' L'habillage d'un contrôle selon sa gravité (Button, Message, ToastMessage) : ses couleurs et ses mesures, liées aux
    ''' jetons du thème. Une valeur que l'application a donnée elle-même à une propriété (une valeur ou une liaison)
    ''' l'emporte : l'habillage ne la remplace pas, dans quelque ordre que les attributs soient lus.
    ''' </summary>
    Friend NotInheritable Class Habillage

        Private ReadOnly _proprietaire As FrameworkElement
        ' Les valeurs simples que l'habillage a posées lui-même (Transparent, une épaisseur), pour les reconnaître.
        Private ReadOnly _posees As New Dictionary(Of DependencyProperty, Object)
        ' Les ressources auxquelles l'habillage a lié chaque propriété : une autre référence vient de l'application.
        Private ReadOnly _liees As New Dictionary(Of DependencyProperty, Object)

        Friend Sub New(proprietaire As FrameworkElement)
            _proprietaire = proprietaire
        End Sub

        ''' <summary>Lie une propriété au jeton <paramref name="jeton"/> du thème, sauf si l'application l'a posée.</summary>
        Friend Sub Lier(propriete As DependencyProperty, jeton As String)
            If DeLApplication(propriete) Then Return
            _posees.Remove(propriete)
            Dim cle = ThemeResources.Prefix & jeton
            _liees(propriete) = cle
            _proprietaire.SetResourceReference(propriete, cle)
        End Sub

        ''' <summary>Pose une valeur, sauf si l'application a posé la sienne.</summary>
        Friend Sub Poser(propriete As DependencyProperty, valeur As Object)
            If DeLApplication(propriete) Then Return
            _posees(propriete) = valeur
            _proprietaire.SetValue(propriete, valeur)
        End Sub

        ' Une valeur locale vient de l'application, sauf la référence à la ressource que l'habillage a posée (un
        ' DynamicResource de l'application, à une autre ressource, est à elle) ou une valeur qu'il a posée lui-même.
        Private Function DeLApplication(propriete As DependencyProperty) As Boolean
            Dim locale = _proprietaire.ReadLocalValue(propriete)
            If locale Is DependencyProperty.UnsetValue Then Return False
            If locale.GetType().Name = "ResourceReferenceExpression" Then
                Dim cle = CleDeLaReference(locale)
                If cle Is Nothing Then Return False
                Dim liee As Object = Nothing
                Return Not (_liees.TryGetValue(propriete, liee) AndAlso Equals(liee, cle))
            End If
            If TypeOf locale Is BindingExpressionBase Then Return True
            Dim posee As Object = Nothing
            Return Not (_posees.TryGetValue(propriete, posee) AndAlso Equals(locale, posee))
        End Function

        ' La clé d'une référence à une ressource (ResourceReferenceExpression, interne à WPF) ; rien si elle ne se lit pas.
        Private Shared Function CleDeLaReference(reference As Object) As Object
            Const Drapeaux = Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.NonPublic
            Dim genre = reference.GetType()
            Dim propriete = genre.GetProperty("ResourceKey", Drapeaux)
            If propriete IsNot Nothing Then Return propriete.GetValue(reference)
            Return genre.GetField("_resourceKey", Drapeaux)?.GetValue(reference)
        End Function

    End Class

End Namespace
