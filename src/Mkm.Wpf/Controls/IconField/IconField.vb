Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>Le côté du champ où se pose l'icône d'un <see cref="IconField"/>.</summary>
    Public Enum IconFieldPosition
        ''' <summary>À gauche, avant le texte (la première InputIcon de mkm).</summary>
        Left
        ''' <summary>À droite, après le texte (la dernière InputIcon de mkm).</summary>
        Right
    End Enum

    ''' <summary>
    ''' Un champ et son icône, dedans (IconField et InputIcon de mkm) : une loupe devant une recherche, un cadenas, une
    ''' unité. L'icône se pose à 7 px du bord ; le champ recule son texte d'autant.
    ''' <code>
    ''' &lt;mkm:IconField Icon="{x:Static mkm:Icons.Search}"&gt;
    '''     &lt;mkm:InputText Placeholder="Rechercher" /&gt;
    ''' &lt;/mkm:IconField&gt;
    ''' </code>
    ''' </summary>
    Public Class IconField
        Inherits ContentControl

        ''' <summary>L'icône : une géométrie de <see cref="Icons"/>.</summary>
        Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Icon), GetType(Object), GetType(IconField), New FrameworkPropertyMetadata(Nothing))

        ''' <summary>Le côté de l'icône : à gauche par défaut.</summary>
        Public Shared ReadOnly IconPositionProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IconPosition), GetType(IconFieldPosition), GetType(IconField),
            New FrameworkPropertyMetadata(IconFieldPosition.Left, AddressOf OnIconPositionChanged))

        ' Le retrait que l'IconField a posé sur son champ : il le retire quand le champ change.
        Private _garni As Control

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(IconField), New FrameworkPropertyMetadata(GetType(IconField)))
            FocusableProperty.OverrideMetadata(GetType(IconField), New FrameworkPropertyMetadata(False))
            IsTabStopProperty.OverrideMetadata(GetType(IconField), New FrameworkPropertyMetadata(False))
        End Sub

        ''' <summary>L'icône.</summary>
        Public Property Icon As Object
            Get
                Return GetValue(IconProperty)
            End Get
            Set(value As Object)
                SetValue(IconProperty, value)
            End Set
        End Property

        ''' <summary>Le côté de l'icône.</summary>
        Public Property IconPosition As IconFieldPosition
            Get
                Return DirectCast(GetValue(IconPositionProperty), IconFieldPosition)
            End Get
            Set(value As IconFieldPosition)
                SetValue(IconPositionProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides Sub OnContentChanged(oldContent As Object, newContent As Object)
            MyBase.OnContentChanged(oldContent, newContent)
            Garnir()
        End Sub

        Private Shared Sub OnIconPositionChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            DirectCast(d, IconField).Garnir()
        End Sub

        ' Le champ recule son texte du côté de l'icône (padding-inline-start chez mkm), sauf retrait posé par
        ' l'application.
        Private Sub Garnir()
            If _garni IsNot Nothing Then
                _garni.ClearValue(PaddingProperty)
                _garni = Nothing
            End If
            Dim champ = TryCast(Content, Control)
            If champ Is Nothing OrElse champ.ReadLocalValue(PaddingProperty) IsNot DependencyProperty.UnsetValue Then Return
            champ.SetResourceReference(PaddingProperty,
                If(IconPosition = IconFieldPosition.Right, "mkm.iconfield.end.padding", "mkm.iconfield.start.padding"))
            _garni = champ
        End Sub

    End Class

End Namespace
