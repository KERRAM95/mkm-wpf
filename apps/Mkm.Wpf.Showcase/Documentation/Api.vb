Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Xml.Linq

Namespace Mkm.Wpf.Showcase.Documentation

    ''' <summary>Une ligne de l'onglet API : une propriété, un évènement, une méthode ou une valeur d'énumération.</summary>
    Friend NotInheritable Class MembreDApi

        ''' <summary>Son nom, tel qu'on l'écrit en XAML ou en VB (« Severity », « ActionBarItem.Label »).</summary>
        Public Property Nom As String

        ''' <summary>Son type, ou sa signature pour une méthode.</summary>
        Public Property Type As String

        ''' <summary>Sa valeur par défaut, pour une propriété de dépendance.</summary>
        Public Property Defaut As String

        ''' <summary>Ce qu'il fait, tiré du commentaire du code.</summary>
        Public Property Description As String

    End Class

    ''' <summary>L'API d'un type de mkm-wpf, rangée comme l'onglet API d'ui-kit.</summary>
    Friend NotInheritable Class ApiDUnType

        Public Property Nom As String
        Public Property Description As String
        Public Property Parent As String
        Public Property EstUneEnumeration As Boolean
        Public Property Proprietes As New List(Of MembreDApi)
        Public Property ProprietesJointes As New List(Of MembreDApi)
        Public Property Evenements As New List(Of MembreDApi)
        Public Property Methodes As New List(Of MembreDApi)
        Public Property Valeurs As New List(Of MembreDApi)

    End Class

    ''' <summary>
    ''' L'onglet API, écrit par le code lui-même : les membres publics d'un type de mkm-wpf, lus par réflexion, et leur
    ''' description, lue dans les commentaires du code (<c>Mkm.Wpf.xml</c>, à côté de la bibliothèque). Il ne peut donc
    ''' pas oublier une propriété ni la décrire autrement que le code.
    ''' </summary>
    Friend NotInheritable Class Api

        Private Shared ReadOnly Bibliotheque As Assembly = GetType(Mkm.Wpf.Controls.Button).Assembly
        Private Shared _commentaires As Dictionary(Of String, XElement)

        Private Sub New()
        End Sub

        ''' <summary>Le type de mkm-wpf de ce nom, cherché dans ses espaces de noms.</summary>
        Public Shared Function TypeNomme(nom As String) As System.Type
            For Each espace In {"Mkm.Wpf.Controls", "Mkm.Wpf.Commands", "Mkm.Wpf.Theming", "Mkm.Wpf.Localization", "Mkm.Wpf"}
                Dim type = Bibliotheque.GetType($"{espace}.{nom}")
                If type IsNot Nothing Then Return type
            Next
            Return Nothing
        End Function

        ''' <summary>L'API d'un type.</summary>
        Public Shared Function Decrire(type As System.Type) As ApiDUnType
            Dim api As New ApiDUnType With {
                .Nom = type.Name,
                .Description = Sommaire(Commentaire("T:" & type.FullName)),
                .EstUneEnumeration = type.IsEnum
            }
            If type.IsEnum Then
                For Each nom In [Enum].GetNames(type)
                    api.Valeurs.Add(New MembreDApi With {.Nom = nom, .Description = Sommaire(Commentaire($"F:{type.FullName}.{nom}"))})
                Next
                Return api
            End If
            If type.BaseType IsNot Nothing AndAlso type.BaseType IsNot GetType(Object) Then api.Parent = NomLisible(type.BaseType, True)

            Const Publics = BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.DeclaredOnly
            Const Partages = BindingFlags.Public Or BindingFlags.Static Or BindingFlags.DeclaredOnly

            For Each p In type.GetProperties(Publics).OrderBy(Function(x) x.Name)
                If p.GetIndexParameters().Length > 0 Then Continue For
                Dim dp = Dependance(type, p.Name)
                Dim lecture = If(p.CanWrite AndAlso p.GetSetMethod() IsNot Nothing, "", " (lecture seule)")
                api.Proprietes.Add(New MembreDApi With {
                    .Nom = p.Name,
                    .Type = NomLisible(p.PropertyType) & lecture,
                    .Defaut = If(dp Is Nothing, "", Valeur(dp.GetMetadata(type).DefaultValue)),
                    .Description = Sommaire(Commentaire($"P:{type.FullName}.{p.Name}"))})
            Next
            For Each p In type.GetProperties(Partages).OrderBy(Function(x) x.Name)
                api.Proprietes.Add(New MembreDApi With {
                    .Nom = $"{type.Name}.{p.Name} (partagée)", .Type = NomLisible(p.PropertyType),
                    .Description = Sommaire(Commentaire($"P:{type.FullName}.{p.Name}"))})
            Next

            ' Une propriété jointe : un champ XProperty et ses méthodes partagées GetX et SetX.
            For Each lire In type.GetMethods(Partages).Where(Function(m) m.Name.StartsWith("Get", StringComparison.Ordinal) AndAlso m.GetParameters().Length = 1).OrderBy(Function(m) m.Name)
                Dim nom = lire.Name.Substring(3)
                Dim dp = Dependance(type, nom)
                If dp Is Nothing OrElse type.GetProperty(nom, Publics) IsNot Nothing Then Continue For
                Dim ecrire = type.GetMethod("Set" & nom, Partages)
                api.ProprietesJointes.Add(New MembreDApi With {
                    .Nom = $"{type.Name}.{nom}",
                    .Type = NomLisible(lire.ReturnType) & If(ecrire Is Nothing, " (lecture seule)", ""),
                    .Defaut = Valeur(dp.DefaultMetadata.DefaultValue),
                    .Description = Sommaire(If(Commentaire(IdDeMethode(lire)), Commentaire($"F:{type.FullName}.{nom}Property")))})
            Next

            For Each e In type.GetEvents(Publics).OrderBy(Function(x) x.Name)
                api.Evenements.Add(New MembreDApi With {
                    .Nom = e.Name, .Type = NomLisible(e.EventHandlerType),
                    .Description = Sommaire(If(Commentaire($"E:{type.FullName}.{e.Name}"), Commentaire($"F:{type.FullName}.{e.Name}Event")))})
            Next

            ' Les méthodes publiques propres au type : ni les redéfinitions, ni les accesseurs des propriétés jointes.
            Dim methodes = type.GetMethods(Publics).Concat(type.GetMethods(Partages)).
                Where(Function(m) Not m.IsSpecialName AndAlso m.GetBaseDefinition().DeclaringType Is type).
                Where(Function(m) Not m.Name.StartsWith("ShouldSerialize", StringComparison.Ordinal)).
                Where(Function(m) Not (m.IsStatic AndAlso m.Name.Length > 3 AndAlso
                                       (m.Name.StartsWith("Get", StringComparison.Ordinal) OrElse m.Name.StartsWith("Set", StringComparison.Ordinal)) AndAlso
                                       Dependance(type, m.Name.Substring(3)) IsNot Nothing)).
                OrderBy(Function(m) m.Name)
            For Each m In methodes
                Dim parametres = String.Join(", ", m.GetParameters().Select(Function(x) $"{x.Name} As {NomLisible(x.ParameterType)}"))
                Dim retour = If(m.ReturnType Is GetType(Void), "", " As " & NomLisible(m.ReturnType))
                api.Methodes.Add(New MembreDApi With {
                    .Nom = If(m.IsStatic, $"{type.Name}.{m.Name}", m.Name),
                    .Type = $"({parametres}){retour}",
                    .Description = Sommaire(Commentaire(IdDeMethode(m)))})
            Next
            Return api
        End Function

        ' Le champ partagé XProperty d'une propriété de dépendance, de ce type ou d'un de ses parents.
        Private Shared Function Dependance(type As System.Type, nom As String) As DependencyProperty
            Dim champ = type.GetField(nom & "Property", BindingFlags.Public Or BindingFlags.Static Or BindingFlags.FlattenHierarchy)
            Return TryCast(champ?.GetValue(Nothing), DependencyProperty)
        End Function

        ' Une valeur par défaut, comme on l'écrirait.
        Private Shared Function Valeur(valeurParDefaut As Object) As String
            If valeurParDefaut Is Nothing Then Return "Nothing"
            If valeurParDefaut Is DependencyProperty.UnsetValue Then Return ""
            Select Case True
                Case TypeOf valeurParDefaut Is String
                    Return $"""{valeurParDefaut}"""
                Case TypeOf valeurParDefaut Is Boolean
                    Return If(CBool(valeurParDefaut), "True", "False")
                Case TypeOf valeurParDefaut Is Double
                    Dim d = CDbl(valeurParDefaut)
                    Return If(Double.IsNaN(d), "NaN (Auto)", d.ToString(CultureInfo.InvariantCulture))
                Case TypeOf valeurParDefaut Is System.Windows.Freezable, TypeOf valeurParDefaut Is System.Windows.Media.Geometry
                    Return ""
                Case Else
                    Dim texte = Convert.ToString(valeurParDefaut, CultureInfo.InvariantCulture)
                    Return If(texte.StartsWith("System.", StringComparison.Ordinal) OrElse texte.StartsWith("Mkm.", StringComparison.Ordinal), "", texte)
            End Select
        End Function

        ''' <summary>Un type comme l'écrit un développeur VB : <c>Boolean</c>, <c>Double?</c>, <c>IList(Of String)</c>.</summary>
        Public Shared Function NomLisible(type As System.Type, Optional complet As Boolean = False) As String
            Dim sousJacent = Nullable.GetUnderlyingType(type)
            If sousJacent IsNot Nothing Then Return NomLisible(sousJacent) & "?"
            If type.IsArray Then Return NomLisible(type.GetElementType()) & "()"
            If type.IsGenericType Then
                Dim nom = type.Name.Substring(0, type.Name.IndexOf("`"c))
                Return $"{nom}(Of {String.Join(", ", type.GetGenericArguments().Select(Function(t) NomLisible(t)))})"
            End If
            Select Case type
                Case GetType(Boolean) : Return "Boolean"
                Case GetType(Integer) : Return "Integer"
                Case GetType(Double) : Return "Double"
                Case GetType(String) : Return "String"
                Case GetType(Object) : Return "Object"
                Case GetType(Date) : Return "Date"
            End Select
            If complet AndAlso type.Namespace IsNot Nothing AndAlso Not type.Namespace.StartsWith("Mkm.", StringComparison.Ordinal) Then
                Return $"{type.Name} (WPF)"
            End If
            Return type.Name
        End Function

        ' ---- Les commentaires du code ----

        Private Shared Function Commentaire(id As String) As XElement
            If _commentaires Is Nothing Then _commentaires = ChargerLesCommentaires()
            Dim element As XElement = Nothing
            Return If(_commentaires.TryGetValue(id, element), element, Nothing)
        End Function

        Private Shared Function ChargerLesCommentaires() As Dictionary(Of String, XElement)
            Dim commentaires As New Dictionary(Of String, XElement)(StringComparer.Ordinal)
            Dim fichier = Path.ChangeExtension(Bibliotheque.Location, ".xml")
            If Not File.Exists(fichier) Then Return commentaires
            For Each membre In XDocument.Load(fichier).Root.Element("members").Elements("member")
                commentaires(CStr(membre.Attribute("name"))) = membre
            Next
            Return commentaires
        End Function

        ' L'identifiant d'une méthode dans le fichier des commentaires : M:Espace.Type.Nom(Type1,Type2).
        Private Shared Function IdDeMethode(m As MethodInfo) As String
            Dim parametres = m.GetParameters()
            Dim id = $"M:{m.DeclaringType.FullName}.{m.Name}"
            If parametres.Length = 0 Then Return id
            Return id & "(" & String.Join(",", parametres.Select(Function(p) IdDeType(p.ParameterType))) & ")"
        End Function

        Private Shared Function IdDeType(type As System.Type) As String
            If type.IsByRef Then Return IdDeType(type.GetElementType()) & "@"
            If type.IsArray Then Return IdDeType(type.GetElementType()) & "[]"
            If type.IsGenericType Then
                Dim nom = type.GetGenericTypeDefinition().FullName
                nom = nom.Substring(0, nom.IndexOf("`"c))
                Return $"{nom}{{{String.Join(",", type.GetGenericArguments().Select(AddressOf IdDeType))}}}"
            End If
            Return type.FullName.Replace("+"c, "."c)
        End Function

        ''' <summary>Le résumé d'un commentaire, en texte : les références par leur nom, les espaces resserrés.</summary>
        Public Shared Function Sommaire(membre As XElement) As String
            Dim resumeXml = membre?.Element("summary")
            If resumeXml Is Nothing Then Return ""
            Dim texte As New StringBuilder()
            Ecrire(resumeXml, texte)
            Return Regex.Replace(texte.ToString(), "\s+", " ").Trim()
        End Function

        Private Shared Sub Ecrire(element As XElement, texte As StringBuilder)
            For Each noeud In element.Nodes()
                Dim t = TryCast(noeud, XText)
                If t IsNot Nothing Then
                    texte.Append(t.Value)
                    Continue For
                End If
                Dim e = TryCast(noeud, XElement)
                If e Is Nothing Then Continue For
                Select Case e.Name.LocalName
                    Case "see", "seealso"
                        Dim cible = If(CStr(e.Attribute("cref")), CStr(e.Attribute("langword")))
                        If e.Nodes().Any() Then
                            Ecrire(e, texte)
                        ElseIf cible IsNot Nothing Then
                            Dim nom = cible.Substring(cible.IndexOf(":"c) + 1)
                            nom = Regex.Replace(nom, "\(.*\)$", "")
                            texte.Append(nom.Substring(nom.LastIndexOf("."c) + 1))
                        End If
                    Case "paramref", "typeparamref"
                        texte.Append(CStr(e.Attribute("name")))
                    Case "para"
                        texte.Append(" ")
                        Ecrire(e, texte)
                        texte.Append(" ")
                    Case Else
                        Ecrire(e, texte)
                End Select
            Next
        End Sub

    End Class

End Namespace
