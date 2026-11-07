Imports System.Windows
Imports System.Windows.Controls

Namespace Mkm.Wpf.Controls

    ''' <summary>
    ''' Le groupe de mkm (<c>Fieldset</c>) : un cadre aux coins du contenu, et sa légende (<c>Header</c>)
    ''' en gras dans un cartouche posé sur le bord du haut, au droit du contenu. Repliable
    ''' (<see cref="Toggleable"/>), sa légende devient un bouton, un plus ou un moins devant elle.
    ''' <code>&lt;mkm:Fieldset Header="Données du cartouche" Toggleable="True"&gt; … &lt;/mkm:Fieldset&gt;</code>
    ''' </summary>
    ''' <remarks>C'est un <see cref="GroupBox"/> : la légende nomme le groupe pour l'accessibilité.</remarks>
    <TemplatePart(Name:=Fieldset.PartLegend, Type:=GetType(FrameworkElement))>
    Public Class Fieldset
        Inherits GroupBox

        ''' <summary>Le nom de la légende dans le gabarit ; elle est placée au droit du contenu.</summary>
        Public Const PartLegend As String = "PART_Legend"

        ''' <summary>Vrai si la légende replie et déplie le contenu.</summary>
        Public Shared ReadOnly ToggleableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(Toggleable), GetType(Boolean), GetType(Fieldset), New FrameworkPropertyMetadata(False))

        ''' <summary>Vrai si le contenu est replié.</summary>
        Public Shared ReadOnly IsCollapsedProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(IsCollapsed), GetType(Boolean), GetType(Fieldset),
            New FrameworkPropertyMetadata(False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault))

        Private _legende As FrameworkElement

        Shared Sub New()
            DefaultStyleKeyProperty.OverrideMetadata(GetType(Fieldset), New FrameworkPropertyMetadata(GetType(Fieldset)))
        End Sub

        ''' <summary>Vrai si la légende replie et déplie le contenu.</summary>
        Public Property Toggleable As Boolean
            Get
                Return CBool(GetValue(ToggleableProperty))
            End Get
            Set(value As Boolean)
                SetValue(ToggleableProperty, value)
            End Set
        End Property

        ''' <summary>Vrai si le contenu est replié.</summary>
        Public Property IsCollapsed As Boolean
            Get
                Return CBool(GetValue(IsCollapsedProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsCollapsedProperty, value)
            End Set
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub OnApplyTemplate()
            MyBase.OnApplyTemplate()
            _legende = TryCast(GetTemplateChild(PartLegend), FrameworkElement)
            PlacerLaLegende()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPropertyChanged(e As DependencyPropertyChangedEventArgs)
            MyBase.OnPropertyChanged(e)
            If e.Property Is PaddingProperty OrElse e.Property Is BorderThicknessProperty Then PlacerLaLegende()
        End Sub

        ' Comme la légende d'un fieldset HTML : au droit du contenu, après la bordure et le retrait de gauche.
        Private Sub PlacerLaLegende()
            If _legende Is Nothing Then Return
            _legende.Margin = New Thickness(BorderThickness.Left + Padding.Left, 0, BorderThickness.Right + Padding.Right, 0)
        End Sub

    End Class

End Namespace
