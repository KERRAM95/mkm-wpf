Imports System.Text.RegularExpressions
Imports System.Windows.Data
Imports System.Windows.Markup

Namespace Mkm.Wpf.Localization

    ''' <summary>
    ''' Un texte traduit en XAML : <c>Text="{mkm:T showcase.title}"</c>. Le texte suit la langue de
    ''' <see cref="Localizer.Current"/>, sans redémarrer.
    ''' </summary>
    <MarkupExtensionReturnType(GetType(String))>
    Public Class TExtension
        Inherits MarkupExtension

        ' Une clé entre dans le chemin d'une liaison (« [clé] ») : ni virgule, ni crochet.
        Private Shared ReadOnly _cleValide As New Regex("^[A-Za-z0-9_.\-]+$")

        ''' <summary>Pour XAML, avec <see cref="Key"/> posée en attribut.</summary>
        Public Sub New()
        End Sub

        ''' <summary>Pour XAML : <c>{mkm:T clé}</c>.</summary>
        Public Sub New(key As String)
            Me.Key = key
        End Sub

        ''' <summary>La clé du texte : lettres, chiffres, « _ », « . » et « - » (« actionBar.more »).</summary>
        <ConstructorArgument("key")>
        Public Property Key As String

        ''' <inheritdoc/>
        Public Overrides Function ProvideValue(serviceProvider As IServiceProvider) As Object
            If Key Is Nothing OrElse Not _cleValide.IsMatch(Key) Then
                Throw New InvalidOperationException($"{{mkm:T}} : clé invalide « {Key} » ; une clé n'a que des lettres, des chiffres, « _ », « . » et « - ».")
            End If

            Dim liaison As New Binding("[" & Key & "]") With {.Source = Localizer.Current, .Mode = BindingMode.OneWay}
            Return liaison.ProvideValue(serviceProvider)
        End Function

    End Class

End Namespace
