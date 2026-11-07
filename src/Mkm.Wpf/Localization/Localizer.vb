Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Data

Namespace Mkm.Wpf.Localization

    ''' <summary>
    ''' La langue de l'application et ses textes. <see cref="Current"/> sert à toute l'application :
    ''' <c>{mkm:T clé}</c> s'y lie, et changer <see cref="Language"/> met à jour tous les textes liés,
    ''' sans redémarrer.
    ''' </summary>
    ''' <remarks>
    ''' Un texte se cherche dans la langue choisie, puis dans sa langue parente (fr pour fr-FR), puis
    ''' en anglais, la langue de repli ; à chaque langue, dans les sources de l'application d'abord,
    ''' dans celles de la bibliothèque ensuite. Absent partout, il vaut sa clé : l'oubli se voit.
    ''' </remarks>
    Public NotInheritable Class Localizer
        Implements INotifyPropertyChanged

        ''' <summary>La langue d'un texte qui manque dans la langue choisie : l'anglais.</summary>
        Public Shared ReadOnly FallbackLanguage As CultureInfo = CultureInfo.GetCultureInfo("en")

        Private Shared ReadOnly _parametre As New Regex("\{(\w+)\}", RegexOptions.Compiled)

        Private ReadOnly _applicationSources As New List(Of ITextSource)
        Private ReadOnly _librarySources As New List(Of ITextSource)
        Private _language As CultureInfo

        ''' <summary>Le <see cref="Localizer"/> de l'application, auquel <c>{mkm:T}</c> se lie.</summary>
        Public Shared ReadOnly Property Current As Localizer = New Localizer()

        ''' <summary>Un <see cref="Localizer"/> à part, dans la langue d'affichage de Windows.</summary>
        Public Sub New()
            _language = CultureInfo.CurrentUICulture
        End Sub

        ''' <inheritdoc/>
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        ''' <summary>Après chaque changement de langue, quand les textes liés sont à jour.</summary>
        Public Event LanguageChanged As EventHandler

        ''' <summary>
        ''' La langue des textes. La changer met à jour tous les textes liés par <c>{mkm:T}</c>.
        ''' Les dates et les nombres, eux, gardent le format régional de Windows.
        ''' </summary>
        Public Property Language As CultureInfo
            Get
                Return _language
            End Get
            Set(value As CultureInfo)
                If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
                If value.Equals(_language) Then Return

                _language = value
                If Me Is Current Then
                    ' Les ressources .NET de l'application suivent la langue choisie.
                    Thread.CurrentThread.CurrentUICulture = value
                    CultureInfo.DefaultThreadCurrentUICulture = value
                End If

                Notifier(NameOf(Language))
                ' « Item[] » : toutes les liaisons à l'indexeur, donc tous les {mkm:T}, se relisent.
                Notifier(Binding.IndexerName)
                RaiseEvent LanguageChanged(Me, EventArgs.Empty)
            End Set
        End Property

        ''' <summary>
        ''' Les langues que l'application propose : celles de ses sources ou, si elle n'en a ajouté
        ''' aucune, celles de la bibliothèque.
        ''' </summary>
        Public ReadOnly Property Languages As IReadOnlyList(Of CultureInfo)
            Get
                Dim sources = If(_applicationSources.Count > 0, _applicationSources, _librarySources)
                Return sources.SelectMany(Function(s) s.Languages).Distinct().ToList().AsReadOnly()
            End Get
        End Property

        ''' <summary>
        ''' Ajoute une source de textes de l'application. Les sources de l'application sont lues dans
        ''' l'ordre de leur ajout, toutes avant celles de la bibliothèque.
        ''' </summary>
        Public Sub AddSource(source As ITextSource)
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If _applicationSources.Contains(source) Then Return

            _applicationSources.Add(source)
            Notifier(NameOf(Languages))
            Notifier(Binding.IndexerName)
        End Sub

        ''' <summary>Retire une source ajoutée par <see cref="AddSource"/>.</summary>
        Public Sub RemoveSource(source As ITextSource)
            If _applicationSources.Remove(source) Then
                Notifier(NameOf(Languages))
                Notifier(Binding.IndexerName)
            End If
        End Sub

        ''' <summary>Ajoute une source de textes de la bibliothèque elle-même.</summary>
        Friend Sub AddLibrarySource(source As ITextSource)
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If _librarySources.Contains(source) Then Return

            _librarySources.Add(source)
            Notifier(NameOf(Languages))
            Notifier(Binding.IndexerName)
        End Sub

        ''' <summary>
        ''' La langue proposée la plus proche de <paramref name="preferred"/> : elle-même, sa langue
        ''' parente (fr pour fr-FR) ou une langue de même parente (fr-FR pour fr-CA), sinon l'anglais,
        ''' sinon la première proposée. Sert à choisir la langue de départ depuis celle de Windows.
        ''' </summary>
        Public Function BestLanguage(preferred As CultureInfo) As CultureInfo
            If preferred Is Nothing Then Throw New ArgumentNullException(NameOf(preferred))

            Dim proposees = Languages
            If proposees.Count = 0 OrElse proposees.Contains(preferred) Then Return preferred

            Dim proche = PremiereDeLaFamille(proposees, If(preferred.IsNeutralCulture, preferred, preferred.Parent))
            If proche IsNot Nothing Then Return proche

            Return If(PremiereDeLaFamille(proposees, FallbackLanguage), proposees(0))
        End Function

        ''' <summary>Le texte d'une clé dans la langue choisie ; c'est à cet indexeur que <c>{mkm:T}</c> se lie.</summary>
        Default Public ReadOnly Property Item(key As String) As String
            Get
                Return Chercher(key)
            End Get
        End Property

        ''' <summary>
        ''' Le texte d'une clé dans la langue choisie, ses paramètres remplacés : par nom
        ''' (<c>T("items.count", New With {.count = 3})</c> pour <c>{count}</c>) ou par position
        ''' (<c>T("items.count", 3)</c> pour <c>{0}</c>).
        ''' </summary>
        Public Function T(key As String, ParamArray args As Object()) As String
            Dim texte = Chercher(key)
            If args Is Nothing OrElse args.Length = 0 Then Return texte
            Return Remplacer(texte, args)
        End Function

        Private Function Chercher(key As String) As String
            If String.IsNullOrEmpty(key) Then Return String.Empty

            Dim sources = _applicationSources.Concat(_librarySources).ToList()
            For Each langue In Langues(sources)
                For Each source In sources
                    Dim texte = source.GetText(key, langue)
                    If texte IsNot Nothing Then Return texte
                Next
            Next
            Return key
        End Function

        ''' <summary>
        ''' Les langues où chercher un texte, dans l'ordre : la langue choisie, ses parentes, l'anglais,
        ''' puis les variantes d'anglais des sources (en-GB…).
        ''' </summary>
        Private Iterator Function Langues(sources As IEnumerable(Of ITextSource)) As IEnumerable(Of CultureInfo)
            Dim vues As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            Dim langue = _language
            While langue IsNot Nothing AndAlso langue.Name.Length > 0
                If vues.Add(langue.Name) Then Yield langue
                langue = langue.Parent
            End While

            If vues.Add(FallbackLanguage.Name) Then Yield FallbackLanguage
            For Each variante In sources.SelectMany(Function(s) s.Languages).Where(Function(l) l.Parent.Equals(FallbackLanguage))
                If vues.Add(variante.Name) Then Yield variante
            Next
        End Function

        Private Shared Function PremiereDeLaFamille(langues As IEnumerable(Of CultureInfo), neutre As CultureInfo) As CultureInfo
            Return langues.FirstOrDefault(Function(l) l.Equals(neutre) OrElse l.Parent.Equals(neutre))
        End Function

        Private Shared Function Remplacer(texte As String, args As Object()) As String
            Dim valeurDe As Func(Of String, Object)

            If args.Length = 1 AndAlso args(0) IsNot Nothing AndAlso Not EstSimple(args(0)) Then
                ' Paramètres nommés : les propriétés d'un objet (New With {.count = 3}).
                Dim objet = args(0)
                valeurDe = Function(nom As String) As Object
                               Dim propriete = objet.GetType().GetProperty(nom, BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.IgnoreCase)
                               Return If(propriete Is Nothing, Nothing, propriete.GetValue(objet))
                           End Function
            Else
                ' Paramètres par position : {0}, {1}…
                valeurDe = Function(nom As String) As Object
                               Dim index As Integer
                               If Integer.TryParse(nom, NumberStyles.None, CultureInfo.InvariantCulture, index) AndAlso index < args.Length Then Return args(index)
                               Return Nothing
                           End Function
            End If

            Return _parametre.Replace(texte, Function(m As Match) As String
                                                 Dim valeur = valeurDe(m.Groups(1).Value)
                                                 Return If(valeur Is Nothing, m.Value, Convert.ToString(valeur, CultureInfo.CurrentCulture))
                                             End Function)
        End Function

        Private Shared Function EstSimple(valeur As Object) As Boolean
            Dim genre = valeur.GetType()
            Return genre.IsPrimitive OrElse genre.IsEnum OrElse TypeOf valeur Is String OrElse TypeOf valeur Is Decimal _
                OrElse TypeOf valeur Is Date OrElse TypeOf valeur Is DateTimeOffset OrElse TypeOf valeur Is TimeSpan OrElse TypeOf valeur Is Guid
        End Function

        Private Sub Notifier(propriete As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propriete))
        End Sub

    End Class

End Namespace
