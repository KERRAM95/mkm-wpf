Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Windows.Markup

' Les styles par défaut des contrôles sont dans Themes/Generic.xaml de cet assembly,
' sans dictionnaire propre à un thème de Windows (Aero, Luna...).
<Assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)>

' Un seul espace de noms XAML pour toute la bibliothèque : xmlns:mkm="urn:mkm-wpf".
' Chaque espace de noms public de l'assembly y est associé par sa propre ligne, dès qu'il a un
' type : le compilateur XAML d'une application refuse un espace de noms vide.
<Assembly: XmlnsPrefix("urn:mkm-wpf", "mkm")>
<Assembly: XmlnsDefinition("urn:mkm-wpf", "Mkm.Wpf")>
<Assembly: XmlnsDefinition("urn:mkm-wpf", "Mkm.Wpf.Commands")>
<Assembly: XmlnsDefinition("urn:mkm-wpf", "Mkm.Wpf.Controls")>
<Assembly: XmlnsDefinition("urn:mkm-wpf", "Mkm.Wpf.Localization")>
<Assembly: XmlnsDefinition("urn:mkm-wpf", "Mkm.Wpf.Theming")>

' Les tests lisent aussi ce qui est réservé à la bibliothèque (Friend).
<Assembly: InternalsVisibleTo("Mkm.Wpf.Tests")>
