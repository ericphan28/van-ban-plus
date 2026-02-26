using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using System.Linq;
using System.Windows.Media;

namespace AIVanBan.Desktop.Views;

// Partial class cho Bulk Actions
public partial class DocumentListPage
{
    // ==================== BULK ACTIONS ====================
    
    private void UpdateBulkActionsUI()
    {
        var selectedCount = dgDocuments.SelectedItems.Count;
        
        if (selectedCount > 0)
        {
            // Show bulk actions toolbar
            bulkActionsPanel.Visibility = Visibility.Visible;
            txtSelectedDocs.Text = $"| Đã chọn: {selectedCount}";
            txtSelectedDocs.Visibility = Visibility.Visible;
        }
        else
        {
            // Hide bulk actions toolbar
            bulkActionsPanel.Visibility = Visibility.Collapsed;
            txtSelectedDocs.Visibility = Visibility.Collapsed;
        }
    }
    
    private void ClearSelection_Click(object sender, RoutedEventArgs e)
    {
        dgDocuments.SelectedItems.Clear();
        UpdateBulkActionsUI();
    }
    
    private void BulkDelete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedDocs = dgDocuments.SelectedItems.Cast<DocumentViewModel>().ToList();
            if (selectedDocs.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một văn bản!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var message = _isTrashView
                ? $"Xóa vĩnh viễn {selectedDocs.Count} văn bản?\n\n⚠️ Không thể hoàn tác!"
                : $"Chuyển {selectedDocs.Count} văn bản vào thùng rác?";
            var title = _isTrashView ? "Xóa vĩnh viễn" : "Xóa hàng loạt";
            
            var result = MessageBox.Show(message, title,
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                int successCount = 0;
                foreach (var docVm in selectedDocs)
                {
                    bool ok = _isTrashView
                        ? _documentService.PermanentDeleteDocument(docVm.Id)
                        : _documentService.SoftDeleteDocument(docVm.Id);
                    if (ok) successCount++;
                }
                
                LoadFolders();
                LoadDocuments();
                
                var doneText = _isTrashView ? "xóa vĩnh viễn" : "chuyển vào thùng rác";
                MessageBox.Show($"✅ Đã {doneText} {successCount}/{selectedDocs.Count} văn bản!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xóa hàng loạt:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void BulkMove_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedDocs = dgDocuments.SelectedItems.Cast<DocumentViewModel>().ToList();
            if (selectedDocs.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một văn bản!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Show folder selection dialog
            var folders = _documentService.GetAllFolders();
            var dialog = new Window
            {
                Title = "Chọn thư mục đích",
                Width = 400,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            
            var tree = new TreeView { Margin = new Thickness(10) };
            BuildFolderTreeForDialog(tree, folders);
            
            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            Grid.SetRow(tree, 0);
            grid.Children.Add(tree);
            
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            Grid.SetRow(btnPanel, 1);
            
            var btnOk = new Button { Content = "Di chuyển", MinWidth = 100, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            var btnCancel = new Button { Content = "Hủy", MinWidth = 100, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center };
            
            btnOk.Click += (s, args) =>
            {
                var selectedItem = tree.SelectedItem as TreeViewItem;
                if (selectedItem?.Tag is FolderNode selectedFolder)
                {
                    dialog.Tag = selectedFolder.Id;
                    dialog.DialogResult = true;
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn thư mục đích!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            
            btnCancel.Click += (s, args) => dialog.Close();
            
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            grid.Children.Add(btnPanel);
            
            dialog.Content = grid;
            
            if (dialog.ShowDialog() == true && dialog.Tag != null)
            {
                string targetFolderId = dialog.Tag.ToString()!;
                int successCount = 0;
                
                foreach (var docVm in selectedDocs)
                {
                    var doc = _documentService.GetDocument(docVm.Id);
                    if (doc != null)
                    {
                        doc.FolderId = targetFolderId;
                        if (_documentService.UpdateDocument(doc))
                            successCount++;
                    }
                }
                
                LoadFolders();
                LoadDocuments();
                
                MessageBox.Show($"✅ Đã di chuyển {successCount}/{selectedDocs.Count} văn bản!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi di chuyển:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void BuildFolderTreeForDialog(TreeView tree, List<Folder> allFolders)
    {
        var rootFolders = allFolders.Where(f => string.IsNullOrEmpty(f.ParentId)).ToList();
        foreach (var folder in rootFolders)
        {
            var node = BuildFolderTreeNodeForDialog(folder, allFolders);
            tree.Items.Add(node);
        }
    }
    
    private TreeViewItem BuildFolderTreeNodeForDialog(Folder folder, List<Folder> allFolders)
    {
        var item = new TreeViewItem
        {
            Header = folder.Name,
            Tag = new FolderNode { Id = folder.Id, Name = folder.Name }
        };
        
        var children = allFolders.Where(f => f.ParentId == folder.Id);
        foreach (var child in children)
        {
            item.Items.Add(BuildFolderTreeNodeForDialog(child, allFolders));
        }
        
        return item;
    }
    
    private void BulkExport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedDocs = dgDocuments.SelectedItems.Cast<DocumentViewModel>().ToList();
            if (selectedDocs.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một văn bản!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = $"Xuất {selectedDocs.Count} văn bản ra Word",
                FileName = $"VanBan_Bulk_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = ".docx",
                Filter = "Word Document (*.docx)|*.docx"
            };
            
            if (saveDialog.ShowDialog() == true)
            {
                var documents = selectedDocs.Select(vm => _documentService.GetDocument(vm.Id))
                                          .Where(d => d != null)
                                          .Cast<Document>()
                                          .ToList();
                
                var wordService = new AIVanBan.Core.Services.WordExportService();
                wordService.ExportMultipleDocuments(documents, saveDialog.FileName);
                
                var result = MessageBox.Show(
                    $"✅ Đã xuất {documents.Count} văn bản ra file:\n{saveDialog.FileName}\n\nBạn có muốn mở file không?",
                    "Xuất Word thành công",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                
                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất Word hàng loạt:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void BulkExportExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedDocs = dgDocuments.SelectedItems.Cast<DocumentViewModel>().ToList();
            if (selectedDocs.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một văn bản!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = $"Xuất {selectedDocs.Count} văn bản ra Excel",
                FileName = $"DanhSach_VanBan_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = ".xlsx",
                Filter = "Excel files (*.xlsx)|*.xlsx"
            };
            
            if (saveDialog.ShowDialog() == true)
            {
                var documents = selectedDocs.Select(vm => _documentService.GetDocument(vm.Id))
                                          .Where(d => d != null)
                                          .Cast<Document>()
                                          .ToList();
                
                var excelService = new ExcelExportService();
                excelService.ExportDocumentList(documents, saveDialog.FileName);
                
                var result = MessageBox.Show(
                    $"✅ Đã xuất {documents.Count} văn bản ra file:\n{saveDialog.FileName}\n\nBạn có muốn mở file không?",
                    "Xuất Excel thành công",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                
                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất Excel hàng loạt:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void BulkTag_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedDocs = dgDocuments.SelectedItems.Cast<DocumentViewModel>().ToList();
            if (selectedDocs.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một văn bản!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var dialog = new Window
            {
                Title = "Gán tag hàng loạt",
                Width = 400,
                Height = 260,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            
            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var lblInfo = new TextBlock
            {
                Text = $"Gán tag cho {selectedDocs.Count} văn bản đã chọn",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(lblInfo, 0);
            
            var txtTags = new TextBox
            {
                Height = 60,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            var lblHint = new TextBlock
            {
                Text = "Nhập các tag (mỗi tag một dòng):",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            var stack = new StackPanel();
            stack.Children.Add(lblHint);
            stack.Children.Add(txtTags);
            Grid.SetRow(stack, 1);
            
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };
            Grid.SetRow(btnPanel, 2);
            
            var btnOk = new Button { Content = "Gán tag", MinWidth = 100, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            var btnCancel = new Button { Content = "Hủy", MinWidth = 100, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center };
            
            btnOk.Click += (s, args) =>
            {
                dialog.Tag = txtTags.Text;
                dialog.DialogResult = true;
                dialog.Close();
            };
            
            btnCancel.Click += (s, args) => dialog.Close();
            
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            
            grid.Children.Add(lblInfo);
            grid.Children.Add(stack);
            grid.Children.Add(btnPanel);
            dialog.Content = grid;
            
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Tag?.ToString()))
            {
                var tagsInput = dialog.Tag.ToString()!;
                var newTags = tagsInput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim())
                                      .Where(t => !string.IsNullOrEmpty(t))
                                      .ToArray();
                
                if (newTags.Length == 0)
                {
                    MessageBox.Show("Vui lòng nhập ít nhất một tag!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                int successCount = 0;
                foreach (var docVm in selectedDocs)
                {
                    var doc = _documentService.GetDocument(docVm.Id);
                    if (doc != null)
                    {
                        var existingTags = doc.Tags?.ToList() ?? new List<string>();
                        foreach (var tag in newTags)
                        {
                            if (!existingTags.Contains(tag))
                                existingTags.Add(tag);
                        }
                        doc.Tags = existingTags.ToArray();
                        
                        if (_documentService.UpdateDocument(doc))
                            successCount++;
                    }
                }
                
                LoadDocuments();
                
                MessageBox.Show($"✅ Đã gán tag cho {successCount}/{selectedDocs.Count} văn bản!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi gán tag:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void BulkChangeType_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedDocs = dgDocuments.SelectedItems.Cast<DocumentViewModel>().ToList();
            if (selectedDocs.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một văn bản!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var dialog = new Window
            {
                Title = "Thay đổi loại văn bản",
                Width = 400,
                Height = 240,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            
            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var lblInfo = new TextBlock
            {
                Text = $"Thay đổi loại cho {selectedDocs.Count} văn bản",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(lblInfo, 0);
            
            var cboType = new ComboBox { Height = 40 };
            cboType.DisplayMemberPath = "Value";
            cboType.SelectedValuePath = "Key";
            foreach (var item in EnumDisplayHelper.GetDocumentTypeItems())
            {
                cboType.Items.Add(item);
            }
            cboType.SelectedIndex = 0;
            
            var lblHint = new TextBlock
            {
                Text = "Chọn loại văn bản mới:",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            var stack = new StackPanel();
            stack.Children.Add(lblHint);
            stack.Children.Add(cboType);
            Grid.SetRow(stack, 1);
            
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };
            Grid.SetRow(btnPanel, 2);
            
            var btnOk = new Button { Content = "Thay đổi", MinWidth = 100, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            var btnCancel = new Button { Content = "Hủy", MinWidth = 100, Height = 36, Padding = new Thickness(16, 0, 16, 0), VerticalContentAlignment = VerticalAlignment.Center };
            
            btnOk.Click += (s, args) =>
            {
                dialog.Tag = cboType.SelectedValue;
                dialog.DialogResult = true;
                dialog.Close();
            };
            
            btnCancel.Click += (s, args) => dialog.Close();
            
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            
            grid.Children.Add(lblInfo);
            grid.Children.Add(stack);
            grid.Children.Add(btnPanel);
            dialog.Content = grid;
            
            if (dialog.ShowDialog() == true && dialog.Tag != null)
            {
                var newType = (DocumentType)dialog.Tag;
                int successCount = 0;
                
                foreach (var docVm in selectedDocs)
                {
                    var doc = _documentService.GetDocument(docVm.Id);
                    if (doc != null)
                    {
                        doc.Type = newType;
                        if (_documentService.UpdateDocument(doc))
                            successCount++;
                    }
                }
                
                LoadDocuments();
                
                MessageBox.Show($"✅ Đã thay đổi loại cho {successCount}/{selectedDocs.Count} văn bản!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi thay đổi loại:\n{ex.Message}", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
