using System.Windows;
using System.Windows.Controls;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

public partial class OrganizationSetupDialog : Window
{
    private readonly OrganizationSetupService _setupService;
    private int _currentStep = 1;
    private OrganizationType _selectedOrgType;
    
    public OrganizationSetupDialog(OrganizationSetupService setupService)
    {
        InitializeComponent();
        _setupService = setupService;
    }
    
    private void OrganizationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        btnNext.IsEnabled = cboOrganizationType.SelectedIndex >= 0;
    }
    
    private void Next_Click(object sender, RoutedEventArgs e)
    {
        _currentStep++;
        UpdateStepUI();
    }
    
    private void Back_Click(object sender, RoutedEventArgs e)
    {
        _currentStep--;
        UpdateStepUI();
    }
    
    private void UpdateStepUI()
    {
        // Hide all steps
        step1Card.Visibility = Visibility.Collapsed;
        step2Card.Visibility = Visibility.Collapsed;
        step3Card.Visibility = Visibility.Collapsed;
        
        // Show current step
        switch (_currentStep)
        {
            case 1:
                step1Card.Visibility = Visibility.Visible;
                btnBack.Visibility = Visibility.Collapsed;
                btnNext.Visibility = Visibility.Visible;
                btnComplete.Visibility = Visibility.Collapsed;
                btnNext.IsEnabled = cboOrganizationType.SelectedIndex >= 0;
                break;
                
            case 2:
                step2Card.Visibility = Visibility.Visible;
                btnBack.Visibility = Visibility.Visible;
                btnNext.Visibility = Visibility.Visible;
                btnComplete.Visibility = Visibility.Collapsed;
                btnNext.IsEnabled = !string.IsNullOrWhiteSpace(txtOrgFullName.Text);
                
                // Auto-fill based on selection
                if (cboOrganizationType.SelectedItem is ComboBoxItem selected)
                {
                    var tag = selected.Tag?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(tag))
                    {
                        _selectedOrgType = Enum.Parse<OrganizationType>(tag);
                        
                        // Auto-suggest names based on organization type
                        // Auto-suggest ký hiệu viết tắt CQ — Theo Phụ lục VI, NĐ 30/2020
                        txtOrgAbbreviation.Text = _selectedOrgType switch
                        {
                            OrganizationType.UbndXa or OrganizationType.UbndTinh => "UBND",
                            OrganizationType.HdndXa or OrganizationType.HdndTinh => "HĐND",
                            OrganizationType.VanPhong => "VP",
                            OrganizationType.TrungTamHanhChinh => "TTHCC",
                            OrganizationType.DangUyXa => "ĐU",
                            OrganizationType.DangUyTinh => "TU",
                            OrganizationType.ChiBoDang => "CB",
                            OrganizationType.DangBo => "ĐB",
                            OrganizationType.BanDanVan => "BDV",
                            OrganizationType.BanToChuc => "BTC",
                            OrganizationType.BanTuyenGiao => "BTG",
                            OrganizationType.BanKiemTra => "UBKT",
                            OrganizationType.BanNoiChinh => "BNC",
                            OrganizationType.BanKinhTe => "BKT",
                            OrganizationType.BanVanHoa => "BVHXH",
                            OrganizationType.MatTran => "UBMTTQ",
                            OrganizationType.HoiNongDan => "HND",
                            OrganizationType.HoiPhuNu => "HPN",
                            OrganizationType.DoanThanhNien => "ĐTN",
                            OrganizationType.HoiCuuChienBinh => "HCCB",
                            OrganizationType.CongDoan => "CĐ",
                            _ => "CQ"
                        };
                        
                        if (string.IsNullOrEmpty(txtOrgFullName.Text))
                        {
                            txtOrgFullName.Text = _selectedOrgType switch
                            {
                                // Chính quyền (2 cấp)
                                OrganizationType.UbndXa => "ỦY BAN NHÂN DÂN XÃ HÒA BÌNH",
                                OrganizationType.UbndTinh => "ỦY BAN NHÂN DÂN TỈNH BẮC NINH",
                                OrganizationType.HdndXa => "HỘI ĐỒNG NHÂN DÂN XÃ HÒA BÌNH",
                                OrganizationType.HdndTinh => "HỘI ĐỒNG NHÂN DÂN TỈNH BẮC NINH",
                                OrganizationType.VanPhong => "VĂN PHÒNG UBND TỈNH BẮC NINH",
                                OrganizationType.TrungTamHanhChinh => "TRUNG TÂM HÀNH CHÍNH CÔNG TỈNH BẮC NINH",
                                
                                // Đảng
                                OrganizationType.DangUyXa => "ĐẢNG ỦY XÃ HÒA BÌNH",
                                OrganizationType.DangUyTinh => "TỈNH ỦY BẮC NINH",
                                OrganizationType.ChiBoDang => "CHI BỘ ĐẢNG CƠ QUAN UBND XÃ",
                                OrganizationType.DangBo => "ĐẢNG BỘ CƠ QUAN",
                                
                                // Ban của Đảng
                                OrganizationType.BanDanVan => "BAN DÂN VẬN TỈNH ỦY BẮC NINH",
                                OrganizationType.BanToChuc => "BAN TỔ CHỨC TỈNH ỦY BẮC NINH",
                                OrganizationType.BanTuyenGiao => "BAN TUYÊN GIÁO TỈNH ỦY BẮC NINH",
                                OrganizationType.BanKiemTra => "BAN KIỂM TRA TỈNH ỦY BẮC NINH",
                                OrganizationType.BanNoiChinh => "BAN NỘI CHÍNH TỈNH ỦY BẮC NINH",
                                OrganizationType.BanKinhTe => "BAN KINH TẾ TỈNH ỦY BẮC NINH",
                                OrganizationType.BanVanHoa => "BAN VĂN HÓA - XÃ HỘI TỈNH ỦY BẮC NINH",
                                
                                // Mặt trận - Đoàn thể
                                OrganizationType.MatTran => "ỦY BAN MẶT TRẬN TỔ QUỐC XÃ HÒA BÌNH",
                                OrganizationType.HoiNongDan => "HỘI NÔNG DÂN XÃ HÒA BÌNH",
                                OrganizationType.HoiPhuNu => "HỘI LIÊN HIỆP PHỤ NỮ XÃ HÒA BÌNH",
                                OrganizationType.DoanThanhNien => "ĐOÀN TNCS HỒ CHÍ MINH XÃ HÒA BÌNH",
                                OrganizationType.HoiCuuChienBinh => "HỘI CỰU CHIẾN BINH XÃ HÒA BÌNH",
                                OrganizationType.CongDoan => "CÔNG ĐOÀN CƠ SỞ UBND XÃ HÒA BÌNH",
                                OrganizationType.HoiChapThap => "HỘI CHỮ THẬP ĐỎ XÃ HÒA BÌNH",
                                OrganizationType.HoiKhuyenHoc => "HỘI KHUYẾN HỌC XÃ HÒA BÌNH",
                                
                                // Sở - Ban - Ngành
                                OrganizationType.SoNoiVu => "SỞ NỘI VỤ TỈNH BẮC NINH",
                                OrganizationType.SoTaiChinh => "SỞ TÀI CHÍNH TỈNH BẮC NINH",
                                OrganizationType.SoKhoHo => "SỞ KẾ HOẠCH VÀ ĐẦU TƯ TỈNH BẮC NINH",
                                OrganizationType.SoGiaoDuc => "SỞ GIÁO DỤC VÀ ĐÀO TẠO TỈNH BẮC NINH",
                                OrganizationType.SoYTe => "SỞ Y TẾ TỈNH BẮC NINH",
                                OrganizationType.SoNongNghiep => "SỞ NÔNG NGHIỆP VÀ PTNT TỈNH BẮC NINH",
                                OrganizationType.SoCongThuong => "SỞ CÔNG THƯƠNG TỈNH BẮC NINH",
                                OrganizationType.SoVanHoa => "SỞ VĂN HÓA, THỂ THAO VÀ DU LỊCH TỈNH BẮC NINH",
                                OrganizationType.SoTaiNguyen => "SỞ TÀI NGUYÊN VÀ MÔI TRƯỜNG TỈNH BẮC NINH",
                                OrganizationType.SoXayDung => "SỞ XÂY DỰNG TỈNH BẮC NINH",
                                OrganizationType.SoGiaoThong => "SỞ GIAO THÔNG VẬN TẢI TỈNH BẮC NINH",
                                OrganizationType.SoTuPhap => "SỞ TƯ PHÁP TỈNH BẮC NINH",
                                OrganizationType.SoThongTin => "SỞ THÔNG TIN VÀ TRUYỀN THÔNG TỈNH BẮC NINH",
                                OrganizationType.SoLaoDong => "SỞ LAO ĐỘNG TBXH TỈNH BẮC NINH",
                                OrganizationType.SoKhoaHoc => "SỞ KHOA HỌC VÀ CÔNG NGHỆ TỈNH BẮC NINH",
                                
                                // Giáo dục & Y tế
                                OrganizationType.TruongMamNon => "TRƯỜNG MẦM NON HÒA BÌNH",
                                OrganizationType.TruongTieuHoc => "TRƯỜNG TIỂU HỌC HÒA BÌNH",
                                OrganizationType.TruongTHCS => "TRƯỜNG THCS HÒA BÌNH",
                                OrganizationType.TruongTHPT => "TRƯỜNG THPT HÒA BÌNH",
                                OrganizationType.TruongDaiHoc => "TRƯỜNG ĐẠI HỌC BẮC NINH",
                                OrganizationType.TramYTe => "TRẠM Y TẾ XÃ HÒA BÌNH",
                                OrganizationType.TrungTamYTe => "TRUNG TÂM Y TẾ THÀNH PHỐ ĐÔNG ANH",
                                OrganizationType.BenhVien => "BỆNH VIỆN ĐA KHOA THÀNH PHỐ ĐÔNG ANH",
                                
                                // Khác
                                OrganizationType.CongAn => "CÔNG AN XÃ HÒA BÌNH",
                                OrganizationType.TrungTamVanHoa => "TRUNG TÂM VĂN HÓA - THỂ THAO THÀNH PHỐ ĐÔNG ANH",
                                OrganizationType.ThuVien => "THƯ VIỆN TỈNH BẮC NINH",
                                OrganizationType.BaoTangVienDi => "BẢO TÀNG TỈNH BẮC NINH",
                                OrganizationType.CongTyNhaNuoc => "CÔNG TY CỔ PHẦN ...",
                                OrganizationType.CoQuanTuyChon => "TÊN CƠ QUAN CỦA BẠN",
                                
                                _ => "TÊN CƠ QUAN"
                            };
                        }
                    }
                }
                break;
                
            case 3:
                step3Card.Visibility = Visibility.Visible;
                btnBack.Visibility = Visibility.Visible;
                btnNext.Visibility = Visibility.Collapsed;
                btnComplete.Visibility = Visibility.Visible;
                break;
        }
    }
    
    private async void Complete_Click(object sender, RoutedEventArgs e)
    {
        ProgressDialog? progressDialog = null;
        
        try
        {
            // Lấy giá trị từ UI controls TRƯỚC KHI chạy background task
            var orgName = txtOrgFullName.Text;
            var orgType = _selectedOrgType;
            var orgAbbreviation = txtOrgAbbreviation.Text?.Trim();
            
            // Disable buttons during operation
            btnComplete.IsEnabled = false;
            btnBack.IsEnabled = false;
            
            // Show progress
            progressDialog = new ProgressDialog("Đang tạo cấu trúc thư mục...");
            progressDialog.Show();
            
            // Run setup với captured variables
            await Task.Run(() =>
            {
                _setupService.CreateDefaultStructure(orgName, orgType, orgAbbreviation);
            });
            
            progressDialog.Close();
            
            MessageBox.Show(
                $"✅ Đã tạo thành công cấu trúc thư mục cho:\n\n{orgName}\n\nHệ thống đã tạo 11 phần chính với hơn 100 thư mục con!",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            if (progressDialog != null && progressDialog.IsVisible)
            {
                progressDialog.Close();
            }
            
            // Sử dụng ErrorDialog với nút Copy
            ErrorDialog.Show(
                "Lỗi khi tạo cấu trúc thư mục", 
                ex.Message, 
                ex.StackTrace);
            
            btnComplete.IsEnabled = true;
            btnBack.IsEnabled = true;
        }
    }
    
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

// Simple progress dialog
public class ProgressDialog : Window
{
    public ProgressDialog(string message)
    {
        Width = 400;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        
        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        panel.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 16,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        });
        
        var progress = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 6
        };
        panel.Children.Add(progress);
        
        Content = panel;
    }
}
