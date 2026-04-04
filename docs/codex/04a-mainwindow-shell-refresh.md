# Umsetzungshilfe – MainWindow.xaml modernisieren

## Ziel
`src/DocuDesk.Desktop/MainWindow.xaml` visuell modernisieren, ohne Bindings, Commands oder die Viewer-Integration zu beschädigen.

## Wichtige Randbedingungen
- `DocumentViewer` mit `x:Name="DocumentViewer"` muss erhalten bleiben.
- Alle bestehenden Bindings und Commands müssen erhalten bleiben.
- Keine Änderungen an Architektur oder ViewModel-Verträgen.
- ResourceDictionaries aus `Styles/DocuDesk.DesignTokens.xaml` und `Styles/DocuDesk.ModernTheme.xaml` lokal in `MainWindow.xaml` einbinden.

## Empfohlene Änderungen
1. `Window.Resources` mit beiden MergedDictionaries ergänzen.
2. `Window` auf den `AppWindowStyle` umstellen.
3. Linke Navigation als dunkle Workspace-Sidebar mit Hero-Block und ruhigen Nav-Buttons gestalten.
4. Obere Such-/Aktionenleiste als große Card mit Titel, Beschreibung, Suchfeld und separaten Primär-/Sekundäraktionen umsetzen.
5. Dokumentliste als Card-Liste mit weicher Selektion umsetzen.
6. Viewer-Bereich als große zentrale Card mit klarer Überschrift und Status-Badge gestalten.
7. Detailbereich in zwei logisch getrennte Cards aufteilen.
8. Job-/Diagnosebereich als ruhige Liste in Card-Optik gestalten.

## Vollständiger XAML-Ersatzvorschlag

```xml
<Window x:Class="DocuDesk.Desktop.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:viewer="clr-namespace:DocuDesk.Viewer;assembly=DocuDesk.Viewer"
        mc:Ignorable="d"
        Title="DocuDesk"
        Height="940"
        Width="1560"
        MinHeight="820"
        MinWidth="1320"
        WindowStartupLocation="CenterScreen"
        Style="{StaticResource AppWindowStyle}">
    <Window.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Styles/DocuDesk.DesignTokens.xaml" />
                <ResourceDictionary Source="Styles/DocuDesk.ModernTheme.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Window.Resources>

    <Grid Background="{StaticResource Brush.AppBackground}">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="284" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0"
                Background="{StaticResource Brush.NavBackground}"
                Padding="22">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="*" />
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>

                <StackPanel>
                    <Border Background="#172554"
                            CornerRadius="14"
                            Padding="14"
                            Margin="0,0,0,18">
                        <StackPanel>
                            <TextBlock Text="DocuDesk"
                                       Foreground="White"
                                       FontSize="28"
                                       FontWeight="Bold" />
                            <TextBlock Text="Dokumente strukturiert bearbeiten"
                                       Foreground="#BFDBFE"
                                       Margin="0,6,0,0"
                                       TextWrapping="Wrap" />
                        </StackPanel>
                    </Border>

                    <TextBlock Text="Navigation"
                               Foreground="#94A3B8"
                               FontSize="12"
                               FontWeight="SemiBold"
                               Margin="6,0,0,10" />

                    <Button Content="Inbox"
                            Style="{StaticResource NavigationButtonStyle}"
                            Margin="0,0,0,8" />
                    <Button Content="Alle Dokumente"
                            Style="{StaticResource NavigationButtonStyle}"
                            Margin="0,0,0,8" />
                    <Button Content="Suche"
                            Style="{StaticResource NavigationButtonStyle}"
                            Margin="0,0,0,8" />
                    <Button Content="Kategorien"
                            Style="{StaticResource NavigationButtonStyle}"
                            Margin="0,0,0,8" />
                    <Button Content="Jobs &amp; Diagnose"
                            Style="{StaticResource NavigationButtonStyle}"
                            Margin="0,0,0,8" />
                    <Button Content="Einstellungen"
                            Style="{StaticResource NavigationButtonStyle}" />
                </StackPanel>

                <Border Grid.Row="2"
                        Background="{StaticResource Brush.NavSurface}"
                        CornerRadius="14"
                        Padding="14">
                    <StackPanel>
                        <TextBlock Text="Arbeitsmodus"
                                   Foreground="White"
                                   FontWeight="SemiBold" />
                        <TextBlock Text="Lokale Verarbeitung, OCR und Viewer in einer Desktop-Oberfläche."
                                   Foreground="#CBD5E1"
                                   Margin="0,8,0,0"
                                   TextWrapping="Wrap" />
                    </StackPanel>
                </Border>
            </Grid>
        </Border>

        <Grid Grid.Column="1" Margin="22">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />
                <RowDefinition Height="*" />
                <RowDefinition Height="260" />
            </Grid.RowDefinitions>

            <Border Grid.Row="0" Style="{StaticResource CardBorderStyle}">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>

                    <StackPanel Margin="0,0,20,0">
                        <TextBlock Text="Dokumenten-Workspace"
                                   Style="{StaticResource SectionTitleTextStyle}" />
                        <TextBlock Text="Suche, importiere und bearbeite Dokumente in einem zusammenhängenden Arbeitsbereich."
                                   Foreground="{StaticResource Brush.TextSecondary}"
                                   Margin="0,6,0,0" />
                        <StackPanel Orientation="Horizontal" Margin="0,18,0,0">
                            <TextBox Width="420"
                                     Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource SearchTextBoxStyle}" />
                            <Button Content="Suchen"
                                    Command="{Binding SearchCommand}"
                                    Style="{StaticResource PrimaryButtonStyle}"
                                    Margin="12,0,0,0" />
                        </StackPanel>
                    </StackPanel>

                    <WrapPanel Grid.Column="1"
                               HorizontalAlignment="Right"
                               VerticalAlignment="Top"
                               Margin="12,0,0,0">
                        <Button Content="Import"
                                Command="{Binding ImportCommand}"
                                Style="{StaticResource PrimaryButtonStyle}"
                                Margin="0,0,10,10" />
                        <Button Content="Jobs verarbeiten"
                                Command="{Binding ProcessJobsCommand}"
                                Style="{StaticResource SecondaryButtonStyle}"
                                Margin="0,0,10,10" />
                        <Button Content="Backup"
                                Command="{Binding BackupCommand}"
                                Style="{StaticResource SecondaryButtonStyle}"
                                Margin="0,0,10,10" />
                        <Button Content="Mail-Entwurf"
                                Command="{Binding DraftMailCommand}"
                                Style="{StaticResource SecondaryButtonStyle}"
                                Margin="0,0,0,10" />
                    </WrapPanel>
                </Grid>
            </Border>

            <Grid Grid.Row="1" Margin="0,18,0,18">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="360" />
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="360" />
                </Grid.ColumnDefinitions>

                <Border Grid.Column="0"
                        Style="{StaticResource CardBorderStyle}"
                        Margin="0,0,14,0">
                    <DockPanel>
                        <StackPanel DockPanel.Dock="Top">
                            <TextBlock Text="Dokumente"
                                       Style="{StaticResource SectionTitleTextStyle}" />
                            <TextBlock Text="Treffer, neue Dokumente und OCR-Status im Überblick."
                                       Foreground="{StaticResource Brush.TextSecondary}"
                                       Margin="0,6,0,0" />
                        </StackPanel>

                        <ListBox ItemsSource="{Binding Documents}"
                                 SelectedItem="{Binding SelectedDocument}"
                                 Margin="0,18,0,0"
                                 Style="{StaticResource DocumentListBoxStyle}"
                                 ItemContainerStyle="{StaticResource DocumentListBoxItemStyle}">
                            <ListBox.ItemTemplate>
                                <DataTemplate>
                                    <Grid>
                                        <Grid.RowDefinitions>
                                            <RowDefinition Height="Auto" />
                                            <RowDefinition Height="Auto" />
                                            <RowDefinition Height="Auto" />
                                        </Grid.RowDefinitions>

                                        <DockPanel LastChildFill="False">
                                            <TextBlock Text="{Binding Title}"
                                                       FontWeight="SemiBold"
                                                       Foreground="{StaticResource Brush.TextPrimary}" />
                                            <Border DockPanel.Dock="Right"
                                                    Background="{StaticResource Brush.AccentSoft}"
                                                    CornerRadius="10"
                                                    Padding="8,4"
                                                    Margin="10,0,0,0">
                                                <TextBlock Text="{Binding OcrStatus}"
                                                           Foreground="{StaticResource Brush.AccentStrong}"
                                                           FontSize="12"
                                                           FontWeight="SemiBold" />
                                            </Border>
                                        </DockPanel>

                                        <StackPanel Grid.Row="1" Margin="0,8,0,0">
                                            <TextBlock Text="{Binding DisplayName}"
                                                       Foreground="{StaticResource Brush.TextSecondary}"
                                                       FontSize="12" />
                                            <TextBlock Text="{Binding ImportedAt}"
                                                       Foreground="{StaticResource Brush.TextMuted}"
                                                       FontSize="12"
                                                       Margin="0,2,0,0" />
                                        </StackPanel>

                                        <TextBlock Grid.Row="2"
                                                   Text="{Binding MatchSnippet}"
                                                   Foreground="{StaticResource Brush.TextSecondary}"
                                                   FontSize="12"
                                                   Margin="0,10,0,0"
                                                   TextWrapping="Wrap" />
                                    </Grid>
                                </DataTemplate>
                            </ListBox.ItemTemplate>
                        </ListBox>
                    </DockPanel>
                </Border>

                <Border Grid.Column="1"
                        Style="{StaticResource CardBorderStyle}"
                        Margin="0,0,14,0">
                    <DockPanel>
                        <Grid DockPanel.Dock="Top" Margin="0,0,0,16">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*" />
                                <ColumnDefinition Width="Auto" />
                            </Grid.ColumnDefinitions>

                            <StackPanel>
                                <TextBlock Text="Viewer"
                                           Style="{StaticResource SectionTitleTextStyle}" />
                                <TextBlock Text="Originaldokument, OCR-Overlay und Annotationen in einer Arbeitsfläche."
                                           Foreground="{StaticResource Brush.TextSecondary}"
                                           Margin="0,6,0,0" />
                            </StackPanel>

                            <Border Grid.Column="1"
                                    Background="{StaticResource Brush.SurfaceAlt}"
                                    BorderBrush="{StaticResource Brush.Border}"
                                    BorderThickness="1"
                                    CornerRadius="10"
                                    Padding="10,6"
                                    VerticalAlignment="Top">
                                <TextBlock Text="Live-Vorschau"
                                           Foreground="{StaticResource Brush.TextMuted}"
                                           FontSize="12"
                                           FontWeight="SemiBold" />
                            </Border>
                        </Grid>

                        <viewer:ViewerHostControl x:Name="DocumentViewer"
                                                  SourcePath="{Binding SelectedDocumentPath}"
                                                  OverlayJson="{Binding SelectedDocumentOverlayJson}"
                                                  HighlightQuery="{Binding SearchText}" />
                    </DockPanel>
                </Border>

                <Border Grid.Column="2"
                        Style="{StaticResource CardBorderStyle}">
                    <StackPanel>
                        <TextBlock Text="Dokumentdetails"
                                   Style="{StaticResource SectionTitleTextStyle}" />
                        <TextBlock Text="Metadaten und Arbeitskontext des ausgewählten Dokuments."
                                   Foreground="{StaticResource Brush.TextSecondary}"
                                   Margin="0,6,0,0" />

                        <Border Background="{StaticResource Brush.SurfaceAlt}"
                                BorderBrush="{StaticResource Brush.Border}"
                                BorderThickness="1"
                                CornerRadius="14"
                                Padding="14"
                                Margin="0,18,0,0">
                            <StackPanel>
                                <TextBlock Text="Titel" Style="{StaticResource BodyLabelTextStyle}" />
                                <TextBlock Text="{Binding SelectedDocument.Title}" TextWrapping="Wrap" Margin="0,4,0,0" />

                                <TextBlock Text="Datei" Style="{StaticResource BodyLabelTextStyle}" Margin="0,14,0,0" />
                                <TextBlock Text="{Binding SelectedDocument.DisplayName}" TextWrapping="Wrap" Margin="0,4,0,0" />

                                <TextBlock Text="Absender" Style="{StaticResource BodyLabelTextStyle}" Margin="0,14,0,0" />
                                <TextBlock Text="{Binding SelectedDocument.Sender}" TextWrapping="Wrap" Margin="0,4,0,0" />

                                <TextBlock Text="Dokumenttyp" Style="{StaticResource BodyLabelTextStyle}" Margin="0,14,0,0" />
                                <TextBlock Text="{Binding SelectedDocument.DocumentType}" TextWrapping="Wrap" Margin="0,4,0,0" />
                            </StackPanel>
                        </Border>

                        <Border Background="{StaticResource Brush.SurfaceAlt}"
                                BorderBrush="{StaticResource Brush.Border}"
                                BorderThickness="1"
                                CornerRadius="14"
                                Padding="14"
                                Margin="0,14,0,0">
                            <StackPanel>
                                <TextBlock Text="Repository-Pfad" Style="{StaticResource BodyLabelTextStyle}" />
                                <TextBlock Text="{Binding SelectedDocument.RepositoryPath}"
                                           TextWrapping="Wrap"
                                           Margin="0,4,0,0"
                                           Foreground="{StaticResource Brush.TextSecondary}" />
                            </StackPanel>
                        </Border>
                    </StackPanel>
                </Border>
            </Grid>

            <Border Grid.Row="2" Style="{StaticResource CardBorderStyle}">
                <DockPanel>
                    <StackPanel DockPanel.Dock="Top">
                        <TextBlock Text="Jobs &amp; Diagnose"
                                   Style="{StaticResource SectionTitleTextStyle}" />
                        <TextBlock Text="Hintergrundverarbeitung, OCR-Lauf und Fehlertexte an einem Ort."
                                   Foreground="{StaticResource Brush.TextSecondary}"
                                   Margin="0,6,0,0" />
                    </StackPanel>

                    <ListBox ItemsSource="{Binding Jobs}" Margin="0,18,0,0" Style="{StaticResource DocumentListBoxStyle}">
                        <ListBox.ItemTemplate>
                            <DataTemplate>
                                <Border Background="{StaticResource Brush.SurfaceAlt}"
                                        BorderBrush="{StaticResource Brush.Border}"
                                        BorderThickness="1"
                                        CornerRadius="14"
                                        Padding="14"
                                        Margin="0,0,0,10">
                                    <Grid>
                                        <Grid.ColumnDefinitions>
                                            <ColumnDefinition Width="220" />
                                            <ColumnDefinition Width="180" />
                                            <ColumnDefinition Width="260" />
                                            <ColumnDefinition Width="*" />
                                        </Grid.ColumnDefinitions>

                                        <TextBlock Grid.Column="0"
                                                   Text="{Binding JobType}"
                                                   FontWeight="SemiBold"
                                                   Foreground="{StaticResource Brush.TextPrimary}" />
                                        <TextBlock Grid.Column="1"
                                                   Text="{Binding Status}"
                                                   Foreground="{StaticResource Brush.AccentStrong}" />
                                        <TextBlock Grid.Column="2"
                                                   Text="{Binding CreatedAt}"
                                                   Foreground="{StaticResource Brush.TextSecondary}" />
                                        <TextBlock Grid.Column="3"
                                                   Text="{Binding ErrorText}"
                                                   TextWrapping="Wrap"
                                                   Foreground="{StaticResource Brush.TextSecondary}" />
                                    </Grid>
                                </Border>
                            </DataTemplate>
                        </ListBox.ItemTemplate>
                    </ListBox>
                </DockPanel>
            </Border>
        </Grid>
    </Grid>
</Window>
```

## Abnahme
Dokumentiere nach der Umsetzung:
- geänderte Dateien
- welche UI-Bereiche modernisiert wurden
- ob alle Bindings/Commands erhalten blieben
- kurze Beschreibung des neuen Layouts
