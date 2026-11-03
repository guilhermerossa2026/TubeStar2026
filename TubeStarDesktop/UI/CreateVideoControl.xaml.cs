using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TubeStar
{
    /// <summary>
    /// Tela "Criar vídeo" (antes AddVideoDialog + AddVideoDialog2)
    /// </summary>
    public partial class CreateVideoControl : UserControl
    {
        //Vídeo criado: a MainPage põe a tarefa na Agenda
        public event Action<Task> VideoCreated;

        private const int ThumbPageSize = 50;
        private static readonly int[] HourOptions = { 2, 3, 4 };
        private static readonly int[] CostOptions = { 0, 1, 2, 3, 4 }; //x 100

        private VideoCategory? _category;
        private int _hours = 2;
        private int _costStep;
        private VideoType? _type;

        //Carrossel de miniaturas
        private int _searchGeneration;
        private string _query;
        private string _lastTitleSearched;
        private List<string> _thumbIds = new List<string>();
        private string _nextPageToken;
        private bool _loadingPage;
        private int _thumbIndex = -1;
        private Dictionary<string, byte[]> _thumbCache = new Dictionary<string, byte[]>();
        private string _shownId;
        private string _chosenId;

        public CreateVideoControl()
        {
            InitializeComponent();
            Translate();
            BuildCategoryChips();
            BuildTypeChips();
            UpdateTypeMatch();
            BuildChips(hoursPanel, HourOptions.Select(h => h + "h").ToArray(), 0, (i) => { _hours = HourOptions[i]; UpdateSummary(); });
            BuildChips(costPanel, CostOptions.Select(c => "$" + (c * 100)).ToArray(), 0, (i) => { _costStep = CostOptions[i]; UpdateSummary(); });
            ResetThumbnails();
            UpdateSummary();
        }

        private void Translate()
        {
            txtTitle.Text = EnglishStrings.NavCreateVideo.Translate();
            txtLead.Text = EnglishStrings.CreateVideoLead.Translate();
            lblName.Text = EnglishStrings.Name.Translate();
            lblCategory.Text = EnglishStrings.NicheLabel.Translate();
            lblThumbnail.Text = EnglishStrings.ThumbnailLabel.Translate();
            btnSearchThumbs.Content = EnglishStrings.ThumbSearch.Translate();
            btnQuerySearch.Content = EnglishStrings.ThumbSearch.Translate();
            btnUseThis.Content = EnglishStrings.ThumbUseThis.Translate();
            btnSearchOthers.Content = EnglishStrings.ThumbSearchOthers.Translate();
            txtThumbHint.Text = EnglishStrings.ThumbHint.Translate();
            lblHours.Text = EnglishStrings.Hours.Translate();
            lblCost.Text = EnglishStrings.Cost.Translate();
            lblType.Text = EnglishStrings.TypeLabel.Translate();
            lblSummary.Text = EnglishStrings.CreateVideoSummary.Translate();
            lblSumCost.Text = EnglishStrings.Cost.Translate();
            lblSumHours.Text = EnglishStrings.Hours.Translate();
            txtCreate.Text = EnglishStrings.NavCreateVideo.Translate();
        }

        //Chamado ao abrir a tela: dinheiro pode ter mudado
        public void Update()
        {
            UpdateSummary();
        }

        #region Chips

        private void BuildChips(WrapPanel panel, string[] labels, int selected, Action<int> onSelect)
        {
            panel.Children.Clear();
            for (int i = 0; i < labels.Length; i++)
            {
                int index = i;
                var chip = new Button();
                chip.Content = labels[i];
                chip.Style = (Style)FindResource(i == selected ? "ChipActiveButtonStyle" : "ChipButtonStyle");
                chip.Click += (s, e) =>
                {
                    SelectChip(panel, chip);
                    onSelect(index);
                };
                panel.Children.Add(chip);
            }
        }

        private void SelectChip(WrapPanel panel, Button selected)
        {
            foreach (Button b in panel.Children)
                b.Style = (Style)FindResource(b == selected ? "ChipActiveButtonStyle" : "ChipButtonStyle");
        }

        private void BuildCategoryChips()
        {
            var data = new List<KeyValuePair<VideoCategory, string>>();
            foreach (VideoCategory category in Enum.GetValues(typeof(VideoCategory)))
                data.Add(new KeyValuePair<VideoCategory, string>(category, category.GetString()));
            data.Sort((l, r) => l.Value.CompareTo(r.Value));

            categoryPanel.Children.Clear();
            foreach (var pair in data)
            {
                var category = pair.Key;
                var chip = new Button();
                chip.Content = pair.Value;
                chip.Tag = category;
                chip.Style = (Style)FindResource("ChipButtonStyle");
                chip.Click += (s, e) =>
                {
                    _category = category;
                    SelectChip(categoryPanel, chip);
                    UpdateTypeMatch();
                };
                categoryPanel.Children.Add(chip);
            }

            if (Settings.LastCategory.HasValue)
                SelectCategory(Settings.LastCategory.Value);
        }

        private void SelectCategory(VideoCategory category)
        {
            foreach (Button b in categoryPanel.Children)
            {
                if ((VideoCategory)b.Tag == category)
                {
                    _category = category;
                    SelectChip(categoryPanel, b);
                }
            }
        }

        #endregion

        #region Tipo de vídeo

        private void BuildTypeChips()
        {
            typePanel.Children.Clear();
            foreach (VideoType type in Enum.GetValues(typeof(VideoType)))
            {
                var t = type;
                var chip = new Button();
                chip.Content = t.GetString();
                chip.Tag = t;
                chip.Style = (Style)FindResource("ChipButtonStyle");
                chip.Click += (s, e) =>
                {
                    _type = t;
                    SelectChip(typePanel, chip);
                    UpdateTypeMatch();
                    UpdateSummary();
                };
                typePanel.Children.Add(chip);
            }
        }

        //Selo "Combina com [nicho]" e lista dos nichos que combinam com o tipo escolhido
        private void UpdateTypeMatch()
        {
            if (!_type.HasValue)
            {
                txtTypeMatch.Text = "";
                txtTypeNiches.Text = EnglishStrings.TypeChooseHint.Translate();
                return;
            }

            txtTypeNiches.Text = String.Format(EnglishStrings.TypeMatchesList.Translate(),
                String.Join(", ", _type.Value.GetMatchingNiches().Select(n => n.GetString())));

            if (!_category.HasValue)
                txtTypeMatch.Text = "";
            else if (_type.Value.Matches(_category.Value))
            {
                txtTypeMatch.Text = "✓ " + String.Format(EnglishStrings.TypeMatches.Translate(), _category.Value.GetString());
                txtTypeMatch.Foreground = (System.Windows.Media.Brush)FindResource("YtGainBrush");
            }
            else
            {
                txtTypeMatch.Text = String.Format(EnglishStrings.TypeNoMatch.Translate(), _category.Value.GetString());
                txtTypeMatch.Foreground = (System.Windows.Media.Brush)FindResource("YtTextSecondaryBrush");
            }
        }

        #endregion

        private void UpdateSummary()
        {
            txtSumCost.Text = "$" + (_costStep * 100);
            txtSumHours.Text = _hours + "h";
            txtSumPoints.Text = String.Format("{0}: {1}", EnglishStrings.TypeLabel.Translate(),
                _type.HasValue ? _type.Value.GetString() : "-");
            txtPreviewName.Text = txtName.Text.Trim();
            txtPreviewThumb.Text = _chosenId != null ? EnglishStrings.ThumbChosen.Translate() : EnglishStrings.ThumbAuto.Translate();
        }

        #region Miniaturas

        private void ResetThumbnails()
        {
            _searchGeneration++;
            _query = null;
            _lastTitleSearched = null;
            _thumbIds.Clear();
            _nextPageToken = null;
            _loadingPage = false;
            _thumbIndex = -1;
            _shownId = null;
            _chosenId = null;
            imgThumb.Source = null;
            imgPreview.Source = null;
            txtThumbStatus.Text = EnglishStrings.ThumbTypeTitle.Translate();
            txtCounter.Text = "";
            txtQueryInfo.Text = "";
            queryPanel.Visibility = Visibility.Collapsed;
            UpdateThumbButtons();
        }

        private void UpdateThumbButtons()
        {
            btnPrev.IsEnabled = _thumbIndex > 0;
            btnNext.IsEnabled = _thumbIndex >= 0 && !_loadingPage && (_thumbIndex + 1 < _thumbIds.Count || _nextPageToken != null);
            btnUseThis.IsEnabled = _thumbIndex >= 0 && _shownId != null && _thumbCache.ContainsKey(_shownId);
        }

        //Só busca ao sair do campo (nunca a cada tecla), e só se o título mudou
        private void txtName_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateSummary();
            string title = txtName.Text.Trim();
            if (title.Length > 0 && title != _lastTitleSearched)
                SearchByTitle();
        }

        private void btnSearchThumbs_Click(object sender, RoutedEventArgs e)
        {
            SearchByTitle();
        }

        private void SearchByTitle()
        {
            string title = txtName.Text.Trim();
            if (title.Length == 0)
            {
                txtThumbStatus.Text = EnglishStrings.ThumbTypeTitle.Translate();
                return;
            }
            _lastTitleSearched = title;
            StartSearch(title, true);
        }

        private void btnSearchOthers_Click(object sender, RoutedEventArgs e)
        {
            queryPanel.Visibility = queryPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            if (queryPanel.Visibility == Visibility.Visible)
            {
                txtQuery.Text = _query ?? txtName.Text.Trim();
                txtQuery.Focus();
                txtQuery.SelectAll();
            }
        }

        private void btnQuerySearch_Click(object sender, RoutedEventArgs e)
        {
            string query = txtQuery.Text.Trim();
            if (query.Length > 0)
                StartSearch(query, false);
        }

        private void txtQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnQuerySearch_Click(sender, e);
        }

        private void StartSearch(string query, bool categoryFallback)
        {
            _searchGeneration++;
            int generation = _searchGeneration;
            _query = query;
            _thumbIds.Clear();
            _nextPageToken = null;
            _thumbIndex = -1;
            _shownId = null;
            imgThumb.Source = null;
            txtCounter.Text = "";
            txtQueryInfo.Text = String.Format(EnglishStrings.ThumbQuery.Translate(), query);

            if (!YouTubeAPI.HasApiKey)
            {
                ShowOffline();
                return;
            }

            txtThumbStatus.Text = EnglishStrings.ThumbSearching.Translate();
            _loadingPage = true;
            UpdateThumbButtons();

            RequestPage(query, null, generation, (found) =>
            {
                if (found)
                {
                    ShowThumb(0);
                }
                else if (categoryFallback && _category.HasValue)
                {
                    //Sem resultado pelo título: usa a categoria, na ordem de relevância
                    _query = _category.Value.GetString();
                    txtQueryInfo.Text = String.Format(EnglishStrings.ThumbQuery.Translate(), _query);
                    _loadingPage = true;
                    RequestPage(_query, null, generation, (found2) =>
                    {
                        if (found2)
                            ShowThumb(0);
                        else
                            ShowNoResults();
                    });
                }
                else
                {
                    ShowNoResults();
                }
            });
        }

        //Uma chamada à API: até 50 ids, guardados na lista
        private void RequestPage(string query, string pageToken, int generation, Action<bool> onDone)
        {
            WebClientHelpers.Download<YouTubeSearchResponse>(YouTubeAPI.SearchVideos(query, ThumbPageSize, pageToken), (response) =>
            {
                if (generation != _searchGeneration)
                    return;
                _loadingPage = false;
                int before = _thumbIds.Count;
                if (response != null && response.Entries != null)
                {
                    foreach (var entry in response.Entries)
                    {
                        if (entry != null && entry.Id != null && !String.IsNullOrEmpty(entry.Id.VideoId) && !_thumbIds.Contains(entry.Id.VideoId))
                            _thumbIds.Add(entry.Id.VideoId);
                    }
                }
                _nextPageToken = response != null && _thumbIds.Count > before ? response.NextPageToken : null;
                onDone(_thumbIds.Count > before);
            }, () =>
            {
                if (generation != _searchGeneration)
                    return;
                _loadingPage = false;
                if (_thumbIds.Count == 0)
                    ShowOffline();
                else
                    UpdateThumbButtons();
            });
        }

        private void ShowOffline()
        {
            _loadingPage = false;
            imgThumb.Source = LoadInternetDown();
            txtThumbStatus.Text = "";
            txtCounter.Text = EnglishStrings.ThumbOffline.Translate();
            UpdateThumbButtons();
        }

        private void ShowNoResults()
        {
            _loadingPage = false;
            imgThumb.Source = null;
            txtThumbStatus.Text = EnglishStrings.ThumbNoResults.Translate();
            UpdateThumbButtons();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_thumbIndex > 0)
                ShowThumb(_thumbIndex - 1);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_thumbIndex + 1 < _thumbIds.Count)
            {
                ShowThumb(_thumbIndex + 1);
            }
            else if (_nextPageToken != null && !_loadingPage)
            {
                //Passou do fim: busca a próxima página (+1 chamada)
                int generation = _searchGeneration;
                int next = _thumbIndex + 1;
                _loadingPage = true;
                txtThumbStatus.Text = EnglishStrings.ThumbSearching.Translate();
                UpdateThumbButtons();
                RequestPage(_query, _nextPageToken, generation, (found) =>
                {
                    if (found)
                        ShowThumb(next);
                    else
                        ShowThumb(_thumbIndex);
                });
            }
        }

        //A imagem só é baixada quando aparece; depois fica guardada
        private void ShowThumb(int index)
        {
            if (index < 0 || index >= _thumbIds.Count)
                return;

            _thumbIndex = index;
            string id = _thumbIds[index];
            _shownId = id;
            txtCounter.Text = String.Format(EnglishStrings.ThumbCounter.Translate(), index + 1, _thumbIds.Count);

            byte[] cached;
            if (_thumbCache.TryGetValue(id, out cached))
            {
                imgThumb.Source = ToBitmap(cached);
                txtThumbStatus.Text = "";
                UpdateThumbButtons();
                return;
            }

            imgThumb.Source = null;
            txtThumbStatus.Text = EnglishStrings.ThumbLoading.Translate();
            UpdateThumbButtons();

            WebClientHelpers.DownloadImage(YouTubeAPI.GetPhotoUri(id), (stream) =>
            {
                byte[] bytes = null;
                try { bytes = StreamHelpers.StreamToBytes(stream); }
                catch { }
                var bitmap = ToBitmap(bytes);
                if (bitmap != null)
                    _thumbCache[id] = bytes;
                if (id != _shownId)
                    return;
                imgThumb.Source = bitmap ?? LoadInternetDown();
                txtThumbStatus.Text = "";
                UpdateThumbButtons();
            }, () =>
            {
                if (id != _shownId)
                    return;
                imgThumb.Source = LoadInternetDown();
                txtThumbStatus.Text = "";
                UpdateThumbButtons();
            });
        }

        private void btnUseThis_Click(object sender, RoutedEventArgs e)
        {
            if (_shownId == null || !_thumbCache.ContainsKey(_shownId))
                return;
            _chosenId = _shownId;
            imgPreview.Source = ToBitmap(_thumbCache[_chosenId]);
            UpdateSummary();
        }

        private static BitmapImage ToBitmap(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = new MemoryStream(bytes);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static BitmapImage LoadInternetDown()
        {
            try
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/InternetDown.jpg"));
            }
            catch
            {
                return null;
            }
        }

        #endregion

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            if (String.IsNullOrEmpty(name))
            {
                CustomMessageBox.ShowDialog(EnglishStrings.MissingValueHeader.Translate(), EnglishStrings.MissingName.Translate(), MessagePicture.Puzzle);
                return;
            }

            if (!_category.HasValue)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.MissingValueHeader.Translate(), EnglishStrings.MissingCategory.Translate(), MessagePicture.Puzzle);
                return;
            }

            if (!_type.HasValue)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.MissingValueHeader.Translate(), EnglishStrings.MissingType.Translate(), MessagePicture.Puzzle);
                return;
            }

            if (Player.Current.Money - (_costStep * 100) < 0)
            {
                CustomMessageBox.ShowDialog(EnglishStrings.LowCashHeader.Translate(), EnglishStrings.LowCashMessage.Translate(), MessagePicture.Money);
                return;
            }

            var video = new Video();
            video.Name = name;
            video.Category = _category.Value;
            video.ExtraShootingHours = _hours - ShootVideo.MinimumShootTime;
            video.Cost = _costStep * 100;
            video.Type = _type.Value;

            //Miniatura: a escolhida; senão a 1ª da busca atual; senão busca pelo título agora
            if (_chosenId != null)
            {
                video.ExternalSetImageBytes(_thumbCache[_chosenId]);
                video.YouTubeVideoId = _chosenId;
            }
            else if (_thumbIds.Count > 0)
                video.SetImageFromId(_thumbIds[0]);
            else
                video.FetchFirstImage();

            Settings.LastCategory = video.Category;

            var shootVideoTask = new ShootVideo(video);
            shootVideoTask.ExtraHours = video.ExtraShootingHours;

            ResetForm();

            if (VideoCreated != null)
                VideoCreated(shootVideoTask);
        }

        private void ResetForm()
        {
            txtName.Text = "";
            _type = null;
            foreach (Button b in typePanel.Children)
                b.Style = (Style)FindResource("ChipButtonStyle");
            _hours = HourOptions[0];
            _costStep = CostOptions[0];
            SelectChip(hoursPanel, (Button)hoursPanel.Children[0]);
            SelectChip(costPanel, (Button)costPanel.Children[0]);
            ResetThumbnails();
            UpdateTypeMatch();
            UpdateSummary();
        }
    }
}
