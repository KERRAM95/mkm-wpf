Imports System.Globalization
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Windows.Documents
Imports System.Windows.Markup
Imports System.Windows.Media
Imports Mkm.Wpf.Localization
Imports Mkm.Wpf.Theming

Namespace Mkm.Wpf

    ''' <summary>
    ''' La configuration de mkm-wpf, posée une fois dans les ressources de l'application :
    ''' <code>
    ''' &lt;Application.Resources&gt;
    '''     &lt;ResourceDictionary&gt;
    '''         &lt;ResourceDictionary.MergedDictionaries&gt;
    '''             &lt;mkm:MkmResources Theme="Ara" Language="fr" /&gt;
    '''         &lt;/ResourceDictionary.MergedDictionaries&gt;
    '''     &lt;/ResourceDictionary&gt;
    ''' &lt;/Application.Resources&gt;
    ''' </code>
    ''' </summary>
    ''' <remarks>
    ''' Elle porte les ressources du thème (<c>mkm.&lt;chemin&gt;</c>). Elle règle aussi deux défauts de
    ''' WPF pour toute l'application : les dates et les nombres prennent le format régional de Windows
    ''' (WPF les écrirait à l'américaine) ; les textes se dessinent nets (<see cref="TextFormattingMode.Display"/>),
    ''' WPF floutant par défaut les petits textes.
    ''' </remarks>
    Public Class MkmResources
        Inherits ResourceDictionary

        Private Shared _reglagesAppliques As Boolean
        Private _ressourcesDuTheme As ResourceDictionary
        Private _theme As ThemeDefinition

        ''' <summary>La configuration, posée dans App.xaml ; son thème est <see cref="Themes.Dashboard"/>.</summary>
        Public Sub New()
            AppliquerReglages()
            Current = Me
            Appliquer(Themes.Dashboard)
            MergedDictionaries.Add(Natifs())
            Controls.MenuIcons.Enregistrer()
            Controls.FocusVisible.Enregistrer()
        End Sub

        ' Les styles que mkm-wpf pose sur des contrôles de WPF eux-mêmes (l'info-bulle…), pour toute l'application.
        Private Shared Function Natifs() As ResourceDictionary
            ' Le constructeur partagé d'Application enregistre le schéma pack://application ; App.xaml l'a déjà fait
            ' tourner, un test ou un hôte sans Application non.
            RuntimeHelpers.RunClassConstructor(GetType(Application).TypeHandle)
            Return New ResourceDictionary With {.Source = New Uri("pack://application:,,,/Mkm.Wpf;component/Themes/Natifs.xaml", UriKind.Absolute)}
        End Function

        ''' <summary>La dernière configuration créée : celle de l'application.</summary>
        Public Shared Property Current As MkmResources

        ''' <summary>
        ''' Le thème, par son nom (« Dashboard », « Ara », ou un thème enregistré par l'application).
        ''' Le changer remplace aussitôt toutes les ressources du thème.
        ''' </summary>
        Public Property Theme As String
            Get
                Return _theme.Name
            End Get
            Set(value As String)
                Dim definition = Themes.Find(value)
                If definition Is Nothing Then
                    Dim connus = String.Join(", ", Themes.All.Select(Function(t) t.Name))
                    Throw New ArgumentException($"Thème inconnu : « {value} ». Thèmes connus : {connus}.", NameOf(value))
                End If
                Appliquer(definition)
            End Set
        End Property

        ''' <summary>
        ''' La langue des textes, par son nom de culture (« fr », « en-GB ») : celle de
        ''' <see cref="Localizer.Current"/>. Sans elle, la langue d'affichage de Windows.
        ''' </summary>
        Public Property Language As String
            Get
                Return Localizer.Current.Language.Name
            End Get
            Set(value As String)
                Localizer.Current.Language = CultureInfo.GetCultureInfo(value)
            End Set
        End Property

        ''' <summary>Remplace les ressources du thème : les <c>DynamicResource</c> suivent.</summary>
        Private Sub Appliquer(definition As ThemeDefinition)
            If definition Is _theme Then Return

            Dim nouvelles = ThemeResources.Build(definition)
            Dim place = If(_ressourcesDuTheme Is Nothing, -1, MergedDictionaries.IndexOf(_ressourcesDuTheme))
            If place < 0 Then
                MergedDictionaries.Add(nouvelles)
            Else
                MergedDictionaries(place) = nouvelles
            End If
            _ressourcesDuTheme = nouvelles
            _theme = definition
        End Sub

        ''' <summary>
        ''' Une fois, avant le premier élément WPF : le format régional de Windows devient la langue
        ''' (<see cref="FrameworkElement.Language"/>) par défaut des éléments, et les textes se
        ''' dessinent en mode <see cref="TextFormattingMode.Display"/>.
        ''' </summary>
        Private Shared Sub AppliquerReglages()
            If _reglagesAppliques Then Return
            _reglagesAppliques = True

            Dim langue = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)
            Remplacer(FrameworkElement.LanguageProperty, New FrameworkPropertyMetadata(langue))
            ' Les textes enrichis (Run, Span, Hyperlink, FlowDocument) ont leur propre défaut, « en-US » : un parent laissé à
            ' son défaut ne leur transmet rien. Le leur aussi suit Windows.
            Remplacer(FrameworkContentElement.LanguageProperty, New FrameworkPropertyMetadata(langue), GetType(TextElement))
            Remplacer(FrameworkContentElement.LanguageProperty, New FrameworkPropertyMetadata(langue), GetType(FlowDocument))
            Remplacer(TextOptions.TextFormattingModeProperty,
                      New FrameworkPropertyMetadata(TextFormattingMode.Display,
                                                    FrameworkPropertyMetadataOptions.AffectsMeasure Or
                                                    FrameworkPropertyMetadataOptions.AffectsRender Or
                                                    FrameworkPropertyMetadataOptions.Inherits))
        End Sub

        Private Shared Sub Remplacer(propriete As DependencyProperty, metadonnee As FrameworkPropertyMetadata, Optional pour As Type = Nothing)
            Try
                propriete.OverrideMetadata(If(pour, GetType(FrameworkElement)), metadonnee)
            Catch ex As InvalidOperationException
                ' Un élément WPF existait déjà (concepteur XAML de Visual Studio) : WPF garde son défaut.
            Catch ex As ArgumentException
                ' La métadonnée était déjà remplacée par l'application : elle a le dernier mot.
            End Try
        End Sub

    End Class

End Namespace
