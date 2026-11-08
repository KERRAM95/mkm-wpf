Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Text.RegularExpressions
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Xml.Linq
Imports Mkm.Wpf.Theming
Imports NUnit.Framework

Namespace Mkm.Wpf.Tests

    ''' <summary>
    ''' Règles que tout contrôle public de Mkm.Wpf doit respecter. Elles s'appliquent
    ''' d'elles-mêmes à chaque contrôle ajouté : il n'y a pas de test à écrire pour cela.
    ''' </summary>
    <TestFixture>
    Public Class ConventionsTests

        Private Const GenericXaml As String = "pack://application:,,,/Mkm.Wpf;component/Themes/Generic.xaml"

        ''' <summary>Les contrôles publics et instanciables de la bibliothèque.</summary>
        Private Shared Function Controles() As IEnumerable(Of Type)
            Return Assembly.Load("Mkm.Wpf").GetExportedTypes().
                Where(Function(t) GetType(Control).IsAssignableFrom(t) AndAlso Not t.IsAbstract).
                OrderBy(Function(t) t.FullName)
        End Function

        <Test>
        Public Sub GenericXaml_SeCharge()
            Dim dictionnaire As New ResourceDictionary With {.Source = New Uri(GenericXaml)}

            Assert.That(dictionnaire.Source, [Is].EqualTo(New Uri(GenericXaml)))
        End Sub

        ''' <summary>
        ''' Sans DefaultStyleKeyProperty.OverrideMetadata dans son constructeur partagé, un
        ''' contrôle prend le style de sa classe parente au lieu du sien.
        ''' </summary>
        <Test>
        Public Sub ChaqueControle_DeclareSaCleDeStyle()
            Dim champ = GetType(FrameworkElement).GetField("DefaultStyleKeyProperty",
                BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic)
            Dim cleDeStyle = DirectCast(champ.GetValue(Nothing), DependencyProperty)

            Dim fautifs = Controles().
                Where(Function(t)
                          ' Le constructeur partagé, qui pose la métadonnée, doit avoir tourné.
                          RuntimeHelpers.RunClassConstructor(t.TypeHandle)
                          Return Not Equals(cleDeStyle.GetMetadata(t).DefaultValue, t)
                      End Function).
                Select(Function(t) t.FullName).
                ToList()

            Assert.That(fautifs, [Is].Empty, "DefaultStyleKeyProperty.OverrideMetadata manquant")
        End Sub

        ''' <summary>
        ''' Chaque contrôle a son style par défaut dans Themes/Generic.xaml (par la fusion
        ''' de son propre dictionnaire).
        ''' </summary>
        <Test>
        Public Sub ChaqueControle_AUnStyleDansGenericXaml()
            Dim dictionnaire As New ResourceDictionary With {.Source = New Uri(GenericXaml)}

            Dim sansStyle = Controles().
                Where(Function(t) TypeOf dictionnaire(t) IsNot Style).
                Select(Function(t) t.FullName).
                ToList()

            Assert.That(sansStyle, [Is].Empty, "style absent de Themes/Generic.xaml")
        End Sub

        ''' <summary>
        ''' Chaque contrôle, créé seul, applique son gabarit et se met en page avec les ressources du thème : un
        ''' gabarit qui casse (une partie absente, une ressource du mauvais type) casse ici. Une fenêtre
        ''' (<see cref="Window"/>) applique seulement son gabarit : elle ne se met en page qu'ouverte.
        ''' </summary>
        <Test>
        Public Sub ChaqueControle_AppliqueSonGabarit()
            Dim fautifs As New List(Of String)
            For Each genre In Controles().Where(Function(t) t.GetConstructor(Type.EmptyTypes) IsNot Nothing)
                Try
                    Dim controle = DirectCast(Activator.CreateInstance(genre), Control)
                    controle.BeginInit()
                    controle.EndInit()
                    If Not controle.ApplyTemplate() AndAlso VisualTreeHelper.GetChildrenCount(controle) = 0 Then
                        fautifs.Add($"{genre.Name} : pas de gabarit")
                        Continue For
                    End If
                    If TypeOf controle Is Window Then
                        DirectCast(controle, Window).Close()
                        Continue For
                    End If
                    controle.Measure(New Size(400, 300))
                    controle.Arrange(New Rect(0, 0, 400, 300))
                    controle.UpdateLayout()
                Catch ex As Exception
                    fautifs.Add($"{genre.Name} : {ex.GetBaseException().Message}")
                End Try
            Next

            Assert.That(fautifs, [Is].Empty)
        End Sub

        ''' <summary>
        ''' Chaque partie annoncée d'un gabarit (<see cref="TemplatePartAttribute"/>), quand le gabarit l'a, est du type
        ''' annoncé : sinon le contrôle ne la trouve pas (<c>TryCast</c> rend <c>Nothing</c>), sans bruit. Le piège : un
        ''' nom de mkm qui cache celui de WPF (<c>ToggleButton</c> dans l'espace de noms des contrôles).
        ''' </summary>
        <Test>
        Public Sub ChaquePartieDeGabarit_EstDuTypeAnnonce()
            Dim fautifs As New List(Of String)
            For Each genre In Controles().Where(Function(t) t.GetConstructor(Type.EmptyTypes) IsNot Nothing AndAlso Not GetType(Window).IsAssignableFrom(t))
                Dim controle = DirectCast(Activator.CreateInstance(genre), Control)
                controle.BeginInit()
                controle.EndInit()
                controle.ApplyTemplate()
                If controle.Template Is Nothing Then Continue For
                For Each partie In genre.GetCustomAttributes(GetType(TemplatePartAttribute), True).Cast(Of TemplatePartAttribute)()
                    Dim trouvee = controle.Template.FindName(partie.Name, controle)
                    If trouvee IsNot Nothing AndAlso Not partie.Type.IsInstanceOfType(trouvee) Then
                        fautifs.Add($"{genre.Name}.{partie.Name} : {trouvee.GetType().FullName}, annoncée {partie.Type.FullName}")
                    End If
                Next
            Next

            Assert.That(fautifs, [Is].Empty)
        End Sub

        ''' <summary>
        ''' Chaque ressource que nomme un gabarit (<c>{DynamicResource mkm.…}</c>) existe dans chaque thème : une
        ''' ressource absente ne lève rien dans WPF, la propriété garde sa valeur par défaut, sans bruit.
        ''' </summary>
        <Test>
        Public Sub ChaqueRessourceDesGabarits_ExisteDansChaqueTheme()
            Dim bibliotheque = DossierDeLaBibliotheque()
            If bibliotheque Is Nothing Then Assert.Ignore("les sources de Mkm.Wpf ne sont pas à côté des tests")

            Dim motif As New Regex("DynamicResource (" & Regex.Escape(ThemeResources.Prefix) & "[A-Za-z0-9.]+)")
            Dim cles = Directory.GetFiles(bibliotheque, "*.xaml", SearchOption.AllDirectories).
                Where(Function(f) Not f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") AndAlso Not f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")).
                SelectMany(Function(f) motif.Matches(File.ReadAllText(f)).Cast(Of Match)().
                    Select(Function(m) $"{m.Groups(1).Value} ({Path.GetFileName(f)})")).
                Distinct().
                ToList()
            Assert.That(cles, [Is].Not.Empty)

            Dim absentes As New List(Of String)
            For Each theme In Themes.All
                Dim ressources = ThemeResources.Build(theme)
                absentes.AddRange(cles.Where(Function(c) Not ressources.Contains(c.Substring(0, c.IndexOf(" "c)))).
                                       Select(Function(c) $"{theme.Name} : {c}"))
            Next

            Assert.That(absentes, [Is].Empty)
        End Sub

        ''' <summary>
        ''' Chaque ressource que nomme un gabarit est, dans chaque thème, du type de la propriété qui la reçoit : WPF ne le
        ''' vérifie pas pour une ressource de gabarit, et la valeur fausse ne casse qu'au rendu (un jeton « …radius »,
        ''' devenu un CornerRadius, sur le RadiusX d'un Rectangle : l'anneau du focus du ToggleButton faisait tomber
        ''' l'application au premier Tab). Les attributs d'un élément et les Setter (sa cible : TargetName, ou le TargetType
        ''' du style ou du gabarit) sont lus.
        ''' </summary>
        <Test>
        Public Sub ChaqueRessourceDesGabarits_ALeTypeDeSaPropriete()
            Dim bibliotheque = DossierDeLaBibliotheque()
            If bibliotheque Is Nothing Then Assert.Ignore("les sources de Mkm.Wpf ne sont pas à côté des tests")

            Dim ressources = Themes.All.Select(Function(t) New KeyValuePair(Of String, ResourceDictionary)(t.Name, ThemeResources.Build(t))).ToList()
            Dim motif As New Regex("^\{DynamicResource (" & Regex.Escape(ThemeResources.Prefix) & "[A-Za-z0-9.]+)\}$")
            Dim fautifs As New List(Of String)
            Dim lus = 0

            For Each fichier In Directory.GetFiles(bibliotheque, "*.xaml", SearchOption.AllDirectories).
                Where(Function(f) Not f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") AndAlso Not f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                Dim document = XDocument.Load(fichier)
                For Each element In document.Descendants()
                    If element.Name.LocalName = "Setter" Then
                        Dim valeur = element.Attribute("Value")
                        Dim m = If(valeur Is Nothing, Nothing, motif.Match(valeur.Value))
                        If m Is Nothing OrElse Not m.Success Then Continue For
                        Dim cible = CibleDuSetter(element)
                        If Verifier(cible, element.Attribute("Property").Value, element, m.Groups(1).Value, ressources, fichier, fautifs) Then lus += 1
                        Continue For
                    End If
                    If element.Name.LocalName.Contains(".") Then Continue For
                    For Each attribut In element.Attributes()
                        Dim m = motif.Match(attribut.Value)
                        If Not m.Success Then Continue For
                        Dim nom = If(attribut.Name.Namespace = XNamespace.None, attribut.Name.LocalName, $"{element.GetPrefixOfNamespace(attribut.Name.Namespace)}:{attribut.Name.LocalName}")
                        If Verifier(TypeDeLElement(element), nom, element, m.Groups(1).Value, ressources, fichier, fautifs) Then lus += 1
                    Next
                Next
            Next

            Assert.That(lus, [Is].GreaterThan(500), "les propriétés reçues ont été trouvées")
            Assert.That(fautifs, [Is].Empty)
        End Sub

        ' Une propriété (« Background », « TextElement.Foreground », « local:FocusVisible.IsVisible ») d'un type : vrai si
        ' elle est une propriété de dépendance connue, et la ressource vérifiée dans chaque thème.
        Private Shared Function Verifier(proprietaire As Type, nom As String, contexte As XElement, cle As String,
                                         ressources As List(Of KeyValuePair(Of String, ResourceDictionary)),
                                         fichier As String, fautifs As List(Of String)) As Boolean
            If proprietaire Is Nothing Then Return False
            Dim nomCourt = nom
            Dim deux = nom.LastIndexOf("."c)
            If deux > 0 Then
                proprietaire = TypeParNom(nom.Substring(0, deux), contexte)
                nomCourt = nom.Substring(deux + 1)
                If proprietaire Is Nothing Then Return False
            End If
            Dim champ = proprietaire.GetField(nomCourt & "Property", BindingFlags.Public Or BindingFlags.Static Or BindingFlags.FlattenHierarchy)
            Dim propriete = TryCast(champ?.GetValue(Nothing), DependencyProperty)
            If propriete Is Nothing Then Return False
            For Each theme In ressources
                If Not theme.Value.Contains(cle) Then Continue For
                Dim valeur = theme.Value(cle)
                If valeur IsNot Nothing AndAlso Not propriete.PropertyType.IsInstanceOfType(valeur) Then
                    fautifs.Add($"{Path.GetFileName(fichier)} : {proprietaire.Name}.{nomCourt} ({propriete.PropertyType.Name}) reçoit {cle}, {valeur.GetType().Name} sous {theme.Key}")
                End If
            Next
            Return True
        End Function

        ' La cible d'un Setter : l'élément nommé par TargetName dans son gabarit, sinon le TargetType du style ou du gabarit
        ' qui le porte.
        Private Shared Function CibleDuSetter(setter As XElement) As Type
            Dim nomCible = setter.Attribute("TargetName")?.Value
            If nomCible IsNot Nothing Then
                Dim gabarit = setter.Ancestors().FirstOrDefault(Function(a) a.Name.LocalName = "ControlTemplate" OrElse a.Name.LocalName = "DataTemplate")
                Dim nomme = gabarit?.Descendants().FirstOrDefault(Function(d) d.Attributes().Any(Function(a) a.Name.LocalName = "Name" AndAlso a.Value = nomCible))
                Return If(nomme Is Nothing, Nothing, TypeDeLElement(nomme))
            End If
            Dim porteur = setter.Ancestors().FirstOrDefault(Function(a) a.Name.LocalName = "Style" OrElse a.Name.LocalName = "ControlTemplate")
            Dim cible = porteur?.Attribute("TargetType")?.Value
            If cible Is Nothing Then Return Nothing
            Dim texte = cible.Trim()
            If texte.StartsWith("{x:Type ", StringComparison.Ordinal) Then texte = texte.Substring(8).TrimEnd("}"c).Trim()
            Return TypeParNom(texte, porteur)
        End Function

        Private Shared Function TypeDeLElement(element As XElement) As Type
            Dim prefixe = element.GetPrefixOfNamespace(element.Name.Namespace)
            Return TypeParNom(If(String.IsNullOrEmpty(prefixe), element.Name.LocalName, prefixe & ":" & element.Name.LocalName), element)
        End Function

        ' Un nom de type de XAML (« Border », « local:Button ») : dans WPF, ou parmi les contrôles de mkm.
        Private Shared Function TypeParNom(nom As String, contexte As XElement) As Type
            Dim prefixe = ""
            Dim court = nom
            Dim deuxPoints = nom.IndexOf(":"c)
            If deuxPoints > 0 Then
                prefixe = nom.Substring(0, deuxPoints)
                court = nom.Substring(deuxPoints + 1)
            End If
            Dim espace = If(prefixe.Length = 0, contexte.GetDefaultNamespace(), contexte.GetNamespaceOfPrefix(prefixe))
            If espace Is Nothing Then Return Nothing
            If espace.NamespaceName.StartsWith("clr-namespace:Mkm.Wpf.Controls", StringComparison.Ordinal) Then
                Return GetType(Mkm.Wpf.Controls.Button).Assembly.GetType("Mkm.Wpf.Controls." & court)
            End If
            If espace.NamespaceName <> "http://schemas.microsoft.com/winfx/2006/xaml/presentation" Then Return Nothing
            Return TypesDeWpf().FirstOrDefault(Function(t) t.Name = court)
        End Function

        Private Shared _typesDeWpf As List(Of Type)

        Private Shared Function TypesDeWpf() As List(Of Type)
            If _typesDeWpf Is Nothing Then
                _typesDeWpf = {GetType(FrameworkElement).Assembly, GetType(UIElement).Assembly, GetType(DependencyObject).Assembly}.
                    SelectMany(Function(a) a.GetExportedTypes()).
                    Where(Function(t) t.Namespace IsNot Nothing AndAlso t.Namespace.StartsWith("System.Windows", StringComparison.Ordinal)).
                    ToList()
            End If
            Return _typesDeWpf
        End Function

        ''' <summary>
        ''' Aucun chemin de liaison avec un préfixe (« (local:ActionBarItem.Icon) ») dans les gabarits : à l'exécution,
        ''' WPF n'en trouvait pas le préfixe, et la liaison échouait sans bruit (les icônes de l'ActionBar). On écrit
        ''' TemplateBinding, ou la liaison en code sur la propriété elle-même.
        ''' </summary>
        <Test>
        Public Sub AucunCheminDeLiaison_AvecUnPrefixe()
            Dim bibliotheque = DossierDeLaBibliotheque()
            If bibliotheque Is Nothing Then Assert.Ignore("les sources de Mkm.Wpf ne sont pas à côté des tests")

            Dim motif As New Regex("\{Binding[^}]*\([A-Za-z]+:[A-Za-z]")
            Dim fautifs = Directory.GetFiles(bibliotheque, "*.xaml", SearchOption.AllDirectories).
                Where(Function(f) Not f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")).
                Where(Function(f) motif.IsMatch(File.ReadAllText(f))).
                Select(Function(f) Path.GetFileName(f)).
                ToList()

            Assert.That(fautifs, [Is].Empty)
        End Sub

        ''' <summary>
        ''' Une partie de gabarit (une classe interne) n'hérite d'un contrôle de mkm que voulu : dans l'espace de noms des
        ''' contrôles, « ToggleButton » est celui de mkm, et non celui de WPF. L'en-tête de l'accordéon et la légende du
        ''' Fieldset en prenaient le style (24 px de haut, 12 px, son fond enfoncé) sous le leur.
        ''' </summary>
        <Test>
        Public Sub ChaquePartieInterne_NHeriteDUnControleDeMkmQueVoulu()
            Dim voulues As New HashSet(Of String) From {"ChevronDeSplitButton", "BoutonDeSpeedDial"}
            Dim bibliotheque = Assembly.Load("Mkm.Wpf")
            Dim fautifs = bibliotheque.GetTypes().
                Where(Function(t) Not t.IsVisible AndAlso GetType(Control).IsAssignableFrom(t)).
                Where(Function(t) t.BaseType IsNot Nothing AndAlso t.BaseType.Assembly Is bibliotheque AndAlso t.BaseType.IsPublic).
                Where(Function(t) Not voulues.Contains(t.Name)).
                Select(Function(t) $"{t.Name} hérite de {t.BaseType.FullName}").
                ToList()

            Assert.That(fautifs, [Is].Empty)
        End Sub

        ' Le dossier de la bibliothèque dans les sources, en remontant depuis les tests ; Nothing hors du dépôt.
        Private Shared Function DossierDeLaBibliotheque() As String
            Dim dossier = New DirectoryInfo(TestContext.CurrentContext.TestDirectory)
            While dossier IsNot Nothing
                Dim candidat = Path.Combine(dossier.FullName, "src", "Mkm.Wpf")
                If Directory.Exists(candidat) Then Return candidat
                dossier = dossier.Parent
            End While
            Return Nothing
        End Function

    End Class

End Namespace
