using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AIVanBan.Core.Models
{
    /// <summary>
    /// Mục trong cây pháp quy (Chương, Mục, Điều, Phụ lục)
    /// </summary>
    public class LegalNode
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public LegalNodeType NodeType { get; set; }
        public string ParentId { get; set; } = "";
        public List<LegalNode> Children { get; set; } = new();

        /// <summary>
        /// Tags ánh xạ đến tính năng trong app (VD: "DocumentType", "Signing", "CopyDocument")
        /// </summary>
        public List<string> FeatureTags { get; set; } = new();

        /// <summary>
        /// Icon gợi ý cho UI
        /// </summary>
        public string Icon { get; set; } = "FileDocument";
    }

    public enum LegalNodeType
    {
        Document,   // Văn bản pháp quy (NĐ 30/2020)
        Chapter,    // Chương
        Section,    // Mục
        Article,    // Điều
        Appendix,   // Phụ lục
        SubSection  // Phần con trong Phụ lục
    }

    /// <summary>
    /// Kết quả tìm kiếm pháp quy
    /// </summary>
    public class LegalSearchResult
    {
        public LegalNode Node { get; set; } = new();
        public string MatchedText { get; set; } = "";
        public string BreadcrumbPath { get; set; } = "";
    }

    /// <summary>
    /// Cung cấp dữ liệu pháp quy NĐ 30/2020/NĐ-CP và 6 Phụ lục
    /// Theo Điều 1-38, NĐ 30/2020/NĐ-CP
    /// </summary>
    public static class LegalReferenceData
    {
        private static List<LegalNode>? _cachedTree;

        /// <summary>
        /// Lấy toàn bộ cây pháp quy NĐ 30/2020
        /// </summary>
        public static List<LegalNode> GetLegalTree()
        {
            if (_cachedTree != null) return _cachedTree;
            _cachedTree = BuildLegalTree();
            return _cachedTree;
        }

        /// <summary>
        /// Tìm kiếm trong pháp quy theo từ khóa
        /// </summary>
        public static List<LegalSearchResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new();

            var results = new List<LegalSearchResult>();
            var tree = GetLegalTree();
            var keywordLower = keyword.ToLower();
            var keywordNoDiacritics = RemoveDiacritics(keywordLower);
            SearchRecursive(tree, keywordLower, keywordNoDiacritics, "", results);
            return results;
        }

        /// <summary>
        /// Tìm Điều theo số (VD: "Điều 8" → LegalNode)
        /// </summary>
        public static LegalNode? FindArticle(int articleNumber)
        {
            var tree = GetLegalTree();
            return FindArticleRecursive(tree, articleNumber);
        }

        /// <summary>
        /// Tìm Phụ lục theo số La Mã (VD: "I", "VI")
        /// </summary>
        public static LegalNode? FindAppendix(string romanNumber)
        {
            var tree = GetLegalTree();
            return tree.SelectMany(t => t.Children)
                       .FirstOrDefault(n => n.NodeType == LegalNodeType.Appendix &&
                                           n.Id.Contains(romanNumber));
        }

        /// <summary>
        /// Lấy danh sách tất cả các Điều (flat list)
        /// </summary>
        public static List<LegalNode> GetAllArticles()
        {
            var tree = GetLegalTree();
            var articles = new List<LegalNode>();
            CollectByType(tree, LegalNodeType.Article, articles);
            return articles;
        }

        /// <summary>
        /// Lấy danh sách tất cả các Phụ lục
        /// </summary>
        public static List<LegalNode> GetAllAppendices()
        {
            var tree = GetLegalTree();
            var appendices = new List<LegalNode>();
            CollectByType(tree, LegalNodeType.Appendix, appendices);
            return appendices;
        }

        #region Private helpers

        private static void SearchRecursive(List<LegalNode> nodes, string keyword, string keywordNoDiacritics, string path, List<LegalSearchResult> results)
        {
            foreach (var node in nodes)
            {
                var currentPath = string.IsNullOrEmpty(path) ? node.Title : $"{path} › {node.Title}";

                var titleLower = node.Title.ToLower();
                var contentLower = node.Content.ToLower();
                
                // Tìm kiếm cả có dấu và không dấu
                bool matched = titleLower.Contains(keyword) || contentLower.Contains(keyword)
                    || RemoveDiacritics(titleLower).Contains(keywordNoDiacritics) 
                    || RemoveDiacritics(contentLower).Contains(keywordNoDiacritics);

                if (matched)
                {
                    // Trích đoạn text match
                    var matchedText = ExtractMatchContext(node.Content, keyword, keywordNoDiacritics);
                    results.Add(new LegalSearchResult
                    {
                        Node = node,
                        MatchedText = matchedText,
                        BreadcrumbPath = currentPath
                    });
                }

                if (node.Children.Count > 0)
                {
                    SearchRecursive(node.Children, keyword, keywordNoDiacritics, currentPath, results);
                }
            }
        }

        private static string ExtractMatchContext(string content, string keyword, string keywordNoDiacritics)
        {
            if (string.IsNullOrEmpty(content)) return "";
            var contentLower = content.ToLower();
            var idx = contentLower.IndexOf(keyword);
            // Fallback: tìm bằng bỏ dấu
            if (idx < 0) idx = RemoveDiacritics(contentLower).IndexOf(keywordNoDiacritics);
            if (idx < 0) return content.Length > 120 ? content[..120] + "..." : content;

            var start = Math.Max(0, idx - 40);
            var end = Math.Min(content.Length, idx + keyword.Length + 80);
            var excerpt = content[start..end].Trim();
            if (start > 0) excerpt = "..." + excerpt;
            if (end < content.Length) excerpt += "...";
            return excerpt;
        }

        /// <summary>
        /// Bỏ dấu tiếng Việt để hỗ trợ tìm kiếm không dấu.
        /// VD: "Nghị định" → "Nghi dinh"
        /// </summary>
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            // Xử lý đ/Đ riêng (không bị normalize FormD tách ra)
            return sb.ToString().Normalize(NormalizationForm.FormC)
                     .Replace('đ', 'd').Replace('Đ', 'D');
        }

        private static LegalNode? FindArticleRecursive(List<LegalNode> nodes, int articleNumber)
        {
            foreach (var node in nodes)
            {
                if (node.NodeType == LegalNodeType.Article && node.Id == $"dieu-{articleNumber}")
                    return node;

                var found = FindArticleRecursive(node.Children, articleNumber);
                if (found != null) return found;
            }
            return null;
        }

        private static void CollectByType(List<LegalNode> nodes, LegalNodeType type, List<LegalNode> results)
        {
            foreach (var node in nodes)
            {
                if (node.NodeType == type) results.Add(node);
                CollectByType(node.Children, type, results);
            }
        }

        #endregion

        #region Build Legal Tree — NĐ 30/2020/NĐ-CP

        private static List<LegalNode> BuildLegalTree()
        {
            var root = new LegalNode
            {
                Id = "nd-30-2020",
                Title = "Nghị định 30/2020/NĐ-CP — Về công tác văn thư",
                Content = "Số hiệu: 30/2020/NĐ-CP\nNgày ban hành: 05/03/2020\nCơ quan ban hành: Chính phủ\nHiệu lực: Còn hiệu lực\n\nNghị định quy định về công tác văn thư và quản lý nhà nước về công tác văn thư. Bao gồm: Soạn thảo, ký ban hành văn bản; quản lý văn bản; lập hồ sơ và nộp lưu hồ sơ, tài liệu vào Lưu trữ cơ quan; quản lý và sử dụng con dấu, thiết bị lưu khóa bí mật trong công tác văn thư.",
                NodeType = LegalNodeType.Document,
                Icon = "BookOpenVariant"
            };

            // ═══════ CHƯƠNG I ═══════
            var chuong1 = new LegalNode
            {
                Id = "chuong-1",
                Title = "Chương I — Quy định chung",
                Content = "Quy định về phạm vi điều chỉnh, đối tượng áp dụng, giải thích từ ngữ, nguyên tắc và yêu cầu quản lý công tác văn thư.",
                NodeType = LegalNodeType.Chapter,
                Icon = "BookOpen"
            };

            chuong1.Children.AddRange(new[]
            {
                CreateArticle(1, "Phạm vi điều chỉnh",
                    "Nghị định này quy định về công tác văn thư và quản lý nhà nước về công tác văn thư. Công tác văn thư bao gồm: Soạn thảo, ký ban hành văn bản; quản lý văn bản; lập hồ sơ và nộp lưu hồ sơ, tài liệu vào Lưu trữ cơ quan; quản lý và sử dụng con dấu, thiết bị lưu khóa bí mật trong công tác văn thư."),

                CreateArticle(2, "Đối tượng áp dụng",
                    "1. Áp dụng đối với cơ quan, tổ chức nhà nước và doanh nghiệp nhà nước.\n2. Tổ chức chính trị, tổ chức chính trị - xã hội, tổ chức xã hội, tổ chức xã hội - nghề nghiệp căn cứ quy định này để áp dụng cho phù hợp."),

                CreateArticle(3, "Giải thích từ ngữ",
                    "1. \"Văn bản\" — thông tin thành văn được truyền đạt bằng ngôn ngữ hoặc ký hiệu.\n" +
                    "2. \"Văn bản chuyên ngành\" — VB hình thành trong hoạt động chuyên môn.\n" +
                    "3. \"Văn bản hành chính\" — VB hình thành trong quá trình chỉ đạo, điều hành.\n" +
                    "4. \"Văn bản điện tử\" — VB dưới dạng thông điệp dữ liệu.\n" +
                    "5. \"Văn bản đi\" — tất cả VB do cơ quan ban hành.\n" +
                    "6. \"Văn bản đến\" — tất cả VB do cơ quan nhận được.\n" +
                    "7. \"Bản thảo văn bản\" — bản viết/đánh máy trong quá trình soạn thảo.\n" +
                    "8. \"Bản gốc văn bản\" — bản hoàn chỉnh, có chữ ký của người có thẩm quyền.\n" +
                    "9. \"Bản chính văn bản giấy\" — bản hoàn chỉnh, tạo từ bản có chữ ký trực tiếp.\n" +
                    "10. \"Bản sao y\" — bản sao đầy đủ, chính xác nội dung bản gốc/bản chính.\n" +
                    "11. \"Bản sao lục\" — bản sao đầy đủ, chính xác nội dung bản sao y.\n" +
                    "12. \"Bản trích sao\" — bản sao chính xác phần nội dung cần trích.\n" +
                    "13. \"Danh mục hồ sơ\" — bảng kê hệ thống hồ sơ dự kiến lập trong năm.\n" +
                    "14. \"Hồ sơ\" — tập hợp VB, tài liệu liên quan về một vấn đề/sự việc.\n" +
                    "15. \"Lập hồ sơ\" — việc tập hợp, sắp xếp VB theo nguyên tắc nhất định.\n" +
                    "16. \"Hệ thống quản lý tài liệu điện tử\" — HTTT tin học hóa công tác văn thư.\n" +
                    "17. \"Văn thư cơ quan\" — bộ phận thực hiện nhiệm vụ công tác văn thư.",
                    new List<string> { "DocumentType", "CopyDocument", "Glossary" }),

                CreateArticle(4, "Nguyên tắc, yêu cầu quản lý công tác văn thư",
                    "1. Nguyên tắc: Công tác văn thư thực hiện thống nhất theo quy định pháp luật.\n\n" +
                    "2. Yêu cầu:\n" +
                    "a) VB phải soạn thảo đúng thẩm quyền, trình tự, thể thức.\n" +
                    "b) Tất cả VB đi/đến phải quản lý tập trung tại Văn thư cơ quan.\n" +
                    "c) VB thuộc ngày nào phải đăng ký, phát hành trong ngày. VB khẩn phải xử lý ngay.\n" +
                    "d) VB phải được theo dõi, cập nhật trạng thái.\n" +
                    "đ) Người giải quyết công việc có trách nhiệm lập hồ sơ và nộp lưu.\n" +
                    "e) Con dấu, thiết bị lưu khóa bí mật phải quản lý theo quy định.\n" +
                    "g) Hệ thống phải đáp ứng Phụ lục VI."),

                CreateArticle(5, "Giá trị pháp lý của văn bản điện tử",
                    "1. VB điện tử được ký số bởi người có thẩm quyền có giá trị pháp lý như bản gốc VB giấy.\n2. Chữ ký số phải đáp ứng đầy đủ quy định pháp luật."),

                CreateArticle(6, "Trách nhiệm của cơ quan, tổ chức, cá nhân",
                    "1. Người đứng đầu cơ quan có trách nhiệm chỉ đạo đúng quy định về văn thư.\n" +
                    "2. Cá nhân phải thực hiện đúng quy định.\n" +
                    "3. Văn thư cơ quan có nhiệm vụ:\na) Đăng ký, phát hành, theo dõi VB đi.\nb) Tiếp nhận, đăng ký, trình VB đến.\nc) Sắp xếp, bảo quản bản lưu.\nd) Quản lý Sổ đăng ký.\nđ) Quản lý con dấu."),
            });

            // ═══════ CHƯƠNG II ═══════
            var chuong2 = new LegalNode
            {
                Id = "chuong-2",
                Title = "Chương II — Soạn thảo, ký ban hành VB hành chính",
                Content = "Quy định về thể thức, kỹ thuật trình bày, quy trình soạn thảo và ký ban hành văn bản hành chính.",
                NodeType = LegalNodeType.Chapter,
                Icon = "FileDocumentEdit"
            };

            chuong2.Children.AddRange(new[]
            {
                CreateArticle(7, "Các loại văn bản hành chính",
                    "Văn bản hành chính gồm 29 loại:\n\n" +
                    "1. Nghị quyết (cá biệt)\n2. Quyết định (cá biệt)\n3. Chỉ thị\n4. Quy chế\n5. Quy định\n" +
                    "6. Thông cáo\n7. Thông báo\n8. Hướng dẫn\n9. Chương trình\n10. Kế hoạch\n" +
                    "11. Phương án\n12. Đề án\n13. Dự án\n14. Báo cáo\n15. Biên bản\n" +
                    "16. Tờ trình\n17. Hợp đồng\n18. Công văn\n19. Công điện\n20. Bản ghi nhớ\n" +
                    "21. Bản thỏa thuận\n22. Giấy ủy quyền\n23. Giấy mời\n24. Giấy giới thiệu\n25. Giấy nghỉ phép\n" +
                    "26. Phiếu gửi\n27. Phiếu chuyển\n28. Phiếu báo\n29. Thư công",
                    new List<string> { "DocumentType", "AICompose" }),

                CreateArticle(8, "Thể thức văn bản",
                    "1. Thể thức văn bản là tập hợp các thành phần cấu thành văn bản.\n\n" +
                    "2. Các thành phần chính (9 thành phần):\n" +
                    "a) Quốc hiệu và Tiêu ngữ.\n" +
                    "b) Tên cơ quan, tổ chức ban hành.\n" +
                    "c) Số, ký hiệu của văn bản.\n" +
                    "d) Địa danh và thời gian ban hành.\n" +
                    "đ) Tên loại và trích yếu nội dung.\n" +
                    "e) Nội dung văn bản.\n" +
                    "g) Chức vụ, họ tên và chữ ký người có thẩm quyền.\n" +
                    "h) Dấu, chữ ký số của cơ quan, tổ chức.\n" +
                    "i) Nơi nhận.\n\n" +
                    "3. Thành phần bổ sung: Phụ lục; dấu mật, mức độ khẩn; ký hiệu người soạn thảo; địa chỉ cơ quan.\n\n" +
                    "4. Chi tiết thể thức thực hiện theo Phụ lục I.",
                    new List<string> { "DocumentEdit", "AICompose", "Template" }),

                CreateArticle(9, "Kỹ thuật trình bày văn bản",
                    "Kỹ thuật trình bày bao gồm: Khổ giấy, kiểu trình bày, định lề trang, phông chữ, cỡ chữ, kiểu chữ, vị trí trình bày, số trang.\n\n" +
                    "• Thể thức trình bày: Theo Phụ lục I\n" +
                    "• Viết hoa: Theo Phụ lục II\n" +
                    "• Chữ viết tắt tên loại VB: Theo Phụ lục III",
                    new List<string> { "Template", "AICompose" }),

                CreateArticle(10, "Soạn thảo văn bản",
                    "1. Người đứng đầu giao đơn vị/cá nhân chủ trì soạn thảo.\n" +
                    "2. Đơn vị/cá nhân soạn thảo: Xác định tên loại, nội dung, độ mật, mức độ khẩn; thu thập xử lý thông tin; soạn đúng thể thức.\n" +
                    "3. Trường hợp sửa đổi: Người có thẩm quyền cho ý kiến vào bản thảo.\n" +
                    "4. Cá nhân soạn thảo chịu trách nhiệm trước pháp luật.",
                    new List<string> { "DocumentEdit", "AICompose" }),

                CreateArticle(11, "Duyệt bản thảo văn bản",
                    "1. Bản thảo phải do người có thẩm quyền ký duyệt.\n2. Bản thảo đã phê duyệt nhưng cần sửa phải trình lại người có thẩm quyền."),

                CreateArticle(12, "Kiểm tra văn bản trước khi ký ban hành",
                    "1. Người đứng đầu đơn vị soạn thảo kiểm tra và chịu trách nhiệm về nội dung.\n" +
                    "2. Người kiểm tra thể thức chịu trách nhiệm về thể thức, kỹ thuật trình bày.",
                    new List<string> { "AIReview" }),

                CreateArticle(13, "Ký ban hành văn bản",
                    "1. Chế độ thủ trưởng: Người đứng đầu ký tất cả VB; có thể giao cấp phó ký thay (KT.).\n" +
                    "2. Chế độ tập thể: Người đứng đầu thay mặt (TM.) tập thể ký. Cấp phó ký thay.\n" +
                    "3. Ký thừa ủy quyền (TUQ.): Phải bằng văn bản, giới hạn thời gian và nội dung.\n" +
                    "4. Ký thừa lệnh (TL.): Giao người đứng đầu đơn vị ký.\n" +
                    "5. Người ký chịu trách nhiệm trước pháp luật.\n" +
                    "6. VB giấy: Dùng bút mực màu xanh.\n" +
                    "7. VB điện tử: Ký số theo Phụ lục I.\n\n" +
                    "⚡ Các quyền hạn ký: TM., KT., TL., TUQ., Q.",
                    new List<string> { "Signing", "DocumentEdit" }),
            });

            // ═══════ CHƯƠNG III ═══════
            var chuong3 = new LegalNode
            {
                Id = "chuong-3",
                Title = "Chương III — Quản lý văn bản",
                Content = "Quy định về quản lý văn bản đi, văn bản đến, và sao văn bản.",
                NodeType = LegalNodeType.Chapter,
                Icon = "FolderOpen"
            };

            // Mục 1: VB đi
            var muc1 = new LegalNode
            {
                Id = "muc-3-1",
                Title = "Mục 1 — Quản lý văn bản đi",
                Content = "Quy định trình tự quản lý VB đi: cấp số, đăng ký, nhân bản, phát hành, lưu.",
                NodeType = LegalNodeType.Section,
                Icon = "EmailSend"
            };

            muc1.Children.AddRange(new[]
            {
                CreateArticle(14, "Trình tự quản lý văn bản đi",
                    "1. Cấp số, thời gian ban hành.\n2. Đăng ký VB đi.\n3. Nhân bản, đóng dấu/ký số.\n4. Phát hành và theo dõi.\n5. Lưu VB đi.",
                    new List<string> { "DocumentList", "Register" }),

                CreateArticle(15, "Cấp số, thời gian ban hành văn bản",
                    "1. Số VB lấy liên tiếp từ số 01 (01/01) đến 31/12 hàng năm. Số và ký hiệu là duy nhất trong một năm.\n" +
                    "a) VB quy phạm pháp luật: Hệ thống số riêng.\n" +
                    "b) VB chuyên ngành: Do người đứng đầu quy định.\n" +
                    "c) VB hành chính: Do người đứng đầu cơ quan quy định.\n\n" +
                    "2. VB giấy: Cấp số sau khi ký, chậm nhất ngày làm việc tiếp theo. VB mật cấp số riêng.\n" +
                    "3. VB điện tử: Cấp số bằng chức năng Hệ thống.",
                    new List<string> { "AutoIncrement", "Register" }),

                CreateArticle(16, "Đăng ký văn bản đi",
                    "1. Đăng ký đầy đủ, chính xác các thông tin.\n" +
                    "2. Đăng ký bằng sổ hoặc Hệ thống:\n" +
                    "a) Bằng sổ: Theo mẫu Phụ lục IV.\n" +
                    "b) Bằng Hệ thống: In ra giấy theo mẫu sổ.\n" +
                    "3. VB mật: Đăng ký theo quy định bảo vệ bí mật.",
                    new List<string> { "Register" }),

                CreateArticle(17, "Nhân bản, đóng dấu, ký số",
                    "1. VB giấy: Nhân bản đúng số lượng nơi nhận. Đóng dấu theo Phụ lục I.\n" +
                    "2. VB điện tử: Ký số theo Phụ lục I."),

                CreateArticle(18, "Phát hành và theo dõi chuyển phát VB đi",
                    "1. VB phải phát hành trong ngày ký, chậm nhất ngày làm việc tiếp theo. VB khẩn: Gửi ngay.\n" +
                    "2. VB mật: Đảm bảo bí mật.\n" +
                    "3. VB sai nội dung → sửa đổi/thay thế. Sai thể thức → đính chính bằng công văn.\n" +
                    "4. Thu hồi VB: Bên nhận gửi lại (giấy) hoặc hủy bỏ trên Hệ thống (điện tử).\n" +
                    "5. Phát hành VB giấy từ VB ký số: In → đóng dấu → tạo bản chính giấy."),

                CreateArticle(19, "Lưu văn bản đi",
                    "1. VB giấy:\na) Bản gốc lưu tại Văn thư cơ quan, đóng dấu, sắp xếp theo thứ tự.\nb) Bản chính lưu tại hồ sơ công việc.\n\n" +
                    "2. VB điện tử:\na) Bản gốc lưu trên Hệ thống.\nb) Hệ thống đáp ứng Phụ lục VI → lưu điện tử thay giấy.\nc) Hệ thống chưa đáp ứng → tạo bản chính giấy.",
                    new List<string> { "Register", "Backup" }),
            });

            // Mục 2: VB đến
            var muc2 = new LegalNode
            {
                Id = "muc-3-2",
                Title = "Mục 2 — Quản lý văn bản đến",
                Content = "Quy định trình tự quản lý VB đến: tiếp nhận, đăng ký, trình, giải quyết.",
                NodeType = LegalNodeType.Section,
                Icon = "EmailReceive"
            };

            muc2.Children.AddRange(new[]
            {
                CreateArticle(20, "Trình tự quản lý văn bản đến",
                    "1. Tiếp nhận VB đến.\n2. Đăng ký VB đến.\n3. Trình, chuyển giao VB đến.\n4. Giải quyết và theo dõi, đôn đốc.",
                    new List<string> { "Register" }),

                CreateArticle(21, "Tiếp nhận văn bản đến",
                    "1. VB giấy:\na) Kiểm tra bì, số lượng, dấu niêm phong.\nb) Bóc bì, đóng dấu \"ĐẾN\". VB gửi đích danh → chuyển không bóc bì.\nc) Mẫu dấu \"ĐẾN\" theo Phụ lục IV.\n\n" +
                    "2. VB điện tử:\na) Kiểm tra tính xác thực, toàn vẹn.\nb) Không đáp ứng → trả lại.\nc) Thông báo nhận VB trong ngày."),

                CreateArticle(22, "Đăng ký văn bản đến",
                    "1. Đăng ký đầy đủ, rõ ràng, chính xác. VB không đăng ký → đơn vị không có trách nhiệm giải quyết.\n" +
                    "2. Số đến lấy liên tiếp trong năm, thống nhất giấy và điện tử.\n" +
                    "3. Đăng ký bằng sổ (Phụ lục IV) hoặc Hệ thống.\n" +
                    "4. VB mật: Đăng ký riêng.",
                    new List<string> { "Register" }),

                CreateArticle(23, "Trình, chuyển giao văn bản đến",
                    "1. Trình trong ngày, chậm nhất ngày làm việc tiếp theo. VB khẩn → trình ngay.\n" +
                    "2. Người có thẩm quyền ghi ý kiến chỉ đạo, xác định đơn vị/cá nhân chủ trì, phối hợp, thời hạn.\n" +
                    "3. VB giấy: Ghi ý kiến vào dấu \"ĐẾN\" hoặc Phiếu giải quyết (Phụ lục IV). Ký nhận khi chuyển giao.\n" +
                    "4. VB điện tử: Trình, chỉ đạo trên Hệ thống."),

                CreateArticle(24, "Giải quyết và theo dõi, đôn đốc VB đến",
                    "1. Người đứng đầu chỉ đạo giải quyết kịp thời, giao người theo dõi, đôn đốc.\n" +
                    "2. Giải quyết theo thời hạn quy chế. VB khẩn → giải quyết ngay.",
                    new List<string> { "DocumentList" }),
            });

            // Mục 3: Sao VB
            var muc3 = new LegalNode
            {
                Id = "muc-3-3",
                Title = "Mục 3 — Sao văn bản",
                Content = "Quy định về các hình thức sao y, sao lục, trích sao và thẩm quyền sao.",
                NodeType = LegalNodeType.Section,
                Icon = "ContentCopy"
            };

            muc3.Children.AddRange(new[]
            {
                CreateArticle(25, "Các hình thức bản sao",
                    "1. Sao y: Giấy→Giấy (chụp), Điện tử→Giấy (in), Giấy→Điện tử (số hóa + ký số).\n" +
                    "2. Sao lục: Từ bản sao y. Giấy↔Giấy, Giấy→Điện tử, Điện tử→Giấy.\n" +
                    "3. Trích sao: Sao phần nội dung cần trích. Tạo lại đầy đủ thể thức.\n" +
                    "4. Thể thức bản sao theo Phụ lục I.",
                    new List<string> { "CopyDocument" }),

                CreateArticle(26, "Giá trị pháp lý của bản sao",
                    "Bản sao y, sao lục và trích sao thực hiện đúng quy định có giá trị pháp lý như bản chính.",
                    new List<string> { "CopyDocument" }),

                CreateArticle(27, "Thẩm quyền sao văn bản",
                    "1. Người đứng đầu cơ quan quyết định việc sao VB và quy định thẩm quyền ký bản sao.\n" +
                    "2. Sao, chụp tài liệu mật theo quy định bảo vệ bí mật nhà nước.",
                    new List<string> { "CopyDocument" }),
            });

            chuong3.Children.Add(muc1);
            chuong3.Children.Add(muc2);
            chuong3.Children.Add(muc3);

            // ═══════ CHƯƠNG IV ═══════
            var chuong4 = new LegalNode
            {
                Id = "chuong-4",
                Title = "Chương IV — Lập hồ sơ và nộp lưu",
                Content = "Quy định về lập Danh mục hồ sơ, lập hồ sơ, nộp lưu vào Lưu trữ cơ quan.",
                NodeType = LegalNodeType.Chapter,
                Icon = "FolderAccount"
            };

            chuong4.Children.AddRange(new[]
            {
                CreateArticle(28, "Lập Danh mục hồ sơ",
                    "Danh mục hồ sơ do người đứng đầu phê duyệt, ban hành đầu năm, gửi các đơn vị/cá nhân. Mẫu theo Phụ lục V."),

                CreateArticle(29, "Lập hồ sơ",
                    "1. Yêu cầu: Phản ánh đúng chức năng; VB trong hồ sơ liên quan chặt chẽ.\n" +
                    "2. Mở hồ sơ: Theo Danh mục hoặc kế hoạch công tác.\n" +
                    "3. Thu thập, cập nhật VB vào hồ sơ bảo đảm toàn vẹn.\n" +
                    "4. Kết thúc hồ sơ:\na) Khi công việc giải quyết xong.\nb) Rà soát, loại bản trùng/nháp, chỉnh sửa tiêu đề.\nc) Hồ sơ giấy: Đánh số tờ, viết Mục lục.\nd) Hồ sơ điện tử: Cập nhật thông tin trên Hệ thống."),

                CreateArticle(30, "Nộp lưu hồ sơ vào Lưu trữ cơ quan",
                    "1. Nộp đủ thành phần, đúng thời hạn.\n" +
                    "2. Thời hạn: Hồ sơ XDCB: 3 tháng từ quyết toán. Hồ sơ khác: 1 năm từ kết thúc.\n" +
                    "3. Thủ tục:\na) Giấy: Lập 2 bản Mục lục + 2 bản Biên bản giao nhận (Phụ lục V).\nb) Điện tử: Nộp lưu trên Hệ thống."),

                CreateArticle(31, "Trách nhiệm lập hồ sơ và nộp lưu",
                    "1. Người đứng đầu quản lý, chỉ đạo, kiểm tra.\n" +
                    "2. Bộ phận hành chính tham mưu, tổ chức thực hiện.\n" +
                    "3. Đơn vị/cá nhân: Lập hồ sơ, bảo quản, nộp lưu HS bảo quản ≥5 năm.\n" +
                    "4. Giữ lại HS: Phải có văn bản đồng ý, tối đa 2 năm.\n" +
                    "5. CBCCVC trước khi nghỉ hưu/chuyển công tác phải bàn giao hồ sơ."),
            });

            // ═══════ CHƯƠNG V ═══════
            var chuong5 = new LegalNode
            {
                Id = "chuong-5",
                Title = "Chương V — Quản lý con dấu",
                Content = "Quy định về quản lý, sử dụng con dấu và thiết bị lưu khóa bí mật.",
                NodeType = LegalNodeType.Chapter,
                Icon = "Stamper"
            };

            chuong5.Children.AddRange(new[]
            {
                CreateArticle(32, "Quản lý con dấu, thiết bị lưu khóa bí mật",
                    "1. Người đứng đầu giao Văn thư cơ quan quản lý.\n" +
                    "2. Văn thư có trách nhiệm:\na) Bảo quản an toàn tại trụ sở.\nb) Chỉ giao khi có văn bản cho phép.\nc) Trực tiếp đóng dấu/ký số.\nd) Chỉ đóng dấu VB đã có chữ ký."),

                CreateArticle(33, "Sử dụng con dấu, thiết bị lưu khóa bí mật",
                    "1. Sử dụng con dấu:\na) Dấu đóng rõ ràng, ngay ngắn, mực đỏ.\nb) Đóng dấu lên chữ ký: Trùm 1/3 về phía bên trái.\nc) VB ban hành kèm: Dấu đóng trang đầu, trùm tên cơ quan.\nd) Dấu treo, dấu giáp lai do người đứng đầu quy định.\nđ) Dấu giáp lai: Mép phải, trùm lên các tờ giấy, tối đa 5 tờ.\n\n" +
                    "2. Thiết bị lưu khóa bí mật dùng để ký số VB điện tử."),
            });

            // ═══════ CHƯƠNG VI, VII ═══════
            var chuong6 = new LegalNode
            {
                Id = "chuong-6",
                Title = "Chương VI — Quản lý nhà nước về công tác văn thư",
                Content = "Nội dung quản lý nhà nước, trách nhiệm quản lý, kinh phí.",
                NodeType = LegalNodeType.Chapter,
                Icon = "AccountBalance"
            };

            chuong6.Children.AddRange(new[]
            {
                CreateArticle(34, "Nội dung quản lý nhà nước",
                    "1. Xây dựng, ban hành VBQPPL.\n2. Quản lý thống nhất nghiệp vụ.\n3. Nghiên cứu khoa học, ứng dụng CNTT.\n4. Đào tạo, bồi dưỡng.\n5. Thanh tra, kiểm tra.\n6. Hợp tác quốc tế.\n7. Sơ kết, tổng kết."),

                CreateArticle(35, "Trách nhiệm quản lý công tác văn thư",
                    "1. Bộ Nội vụ chịu trách nhiệm trước Chính phủ.\n2. Các bộ, UBND các cấp: Ban hành, hướng dẫn, kiểm tra, bố trí kinh phí, đào tạo."),

                CreateArticle(36, "Kinh phí cho công tác văn thư",
                    "1. Bố trí kinh phí trong dự toán ngân sách hàng năm.\n2. Sử dụng: Mua sắm hạ tầng, thông tin liên lạc, chuyển phát, nghiên cứu."),

                CreateArticle(37, "Hiệu lực thi hành",
                    "Có hiệu lực từ ngày ký (05/03/2020). Thay thế NĐ 110/2004/NĐ-CP và NĐ 09/2010/NĐ-CP."),

                CreateArticle(38, "Trách nhiệm thi hành",
                    "Bộ trưởng Bộ Nội vụ triển khai và kiểm tra. Các Bộ trưởng, Thủ trưởng, Chủ tịch UBND chịu trách nhiệm thi hành."),
            });

            // ═══════ PHỤ LỤC ═══════
            var phuLuc1 = CreateAppendix("I", "Thể thức, kỹ thuật trình bày VB hành chính",
                "QUY ĐỊNH CHUNG:\n" +
                "• Khổ giấy: A4 (210 x 297 mm)\n" +
                "• Kiểu trình bày: Theo chiều dài A4\n" +
                "• Định lề: Trên/dưới 20-25mm, trái 30-35mm, phải 15-20mm\n" +
                "• Phông chữ: Times New Roman, Unicode, màu đen\n\n" +
                "CÁC THÀNH PHẦN THỂ THỨC CHÍNH:\n" +
                "1. Quốc hiệu và Tiêu ngữ — In hoa, cỡ 12-13, đậm\n" +
                "   Tiêu ngữ: \"Độc lập - Tự do - Hạnh phúc\" — In thường, cỡ 13-14, đậm\n" +
                "2. Tên cơ quan ban hành — In hoa, cỡ 12-13, đậm, canh giữa\n" +
                "3. Số, ký hiệu — Cỡ 13, đứng. Format: Số: XX/Loại-CQ\n" +
                "4. Địa danh và thời gian — Cỡ 13-14, nghiêng. VD: Hà Nội, ngày 05 tháng 3 năm 2020\n" +
                "5. Tên loại và trích yếu — Tên loại: In hoa, đậm. Trích yếu: In thường, đậm\n" +
                "6. Nội dung VB — In thường, cỡ 13-14, canh đều 2 lề, lùi đầu dòng 1-1.27cm\n" +
                "7. Chức vụ, họ tên, chữ ký — Quyền hạn (TM., KT., TL., TUQ.): In hoa, đậm\n" +
                "8. Dấu, chữ ký số — Dấu đỏ trùm 1/3 chữ ký. Ký số: PNG nền trong suốt\n" +
                "9. Nơi nhận — \"Nơi nhận:\": In thường, nghiêng, đậm, cỡ 12\n\n" +
                "SƠ ĐỒ BỐ TRÍ: 14 ô vị trí trình bày trên khổ A4");
            phuLuc1.Children.AddRange(new[]
            {
                new LegalNode { Id = "pl1-quoc-hieu", Title = "Quốc hiệu và Tiêu ngữ", NodeType = LegalNodeType.SubSection,
                    Content = "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM — In hoa, cỡ 12-13, đứng, đậm, phía trên cùng bên phải.\nĐộc lập - Tự do - Hạnh phúc — In thường, cỡ 13-14, đứng, đậm, canh giữa dưới Quốc hiệu, có đường kẻ ngang phía dưới." },
                new LegalNode { Id = "pl1-so-ky-hieu", Title = "Số, ký hiệu văn bản", NodeType = LegalNodeType.SubSection,
                    Content = "Số ghi bằng chữ số Ả Rập. Số < 10: thêm 0 phía trước (01, 02...).\nFormat: Số: XX/Loại-CQ (VD: Số: 30/NĐ-CP)\nGiữa số và ký hiệu có dấu gạch chéo (/), giữa các nhóm chữ viết tắt có gạch nối (-), không cách chữ.\nCỡ chữ 13, kiểu đứng, canh giữa dưới tên cơ quan." },
                new LegalNode { Id = "pl1-noi-dung", Title = "Nội dung văn bản", NodeType = LegalNodeType.SubSection,
                    Content = "Canh đều 2 lề, cỡ chữ 13-14, kiểu đứng.\nLùi đầu dòng 1 cm hoặc 1,27 cm.\nKhoảng cách đoạn: tối thiểu 6pt.\nKhoảng cách dòng: đơn đến 1,5 lines.\n\nBố cục: Phần → Chương → Mục → Tiểu mục → Điều → Khoản → Điểm\nĐiểm dùng chữ cái tiếng Việt (a, b, c...) + dấu đóng ngoặc đơn." },
                new LegalNode { Id = "pl1-chu-ky", Title = "Chữ ký và quyền hạn", NodeType = LegalNodeType.SubSection,
                    Content = "Quyền hạn ký: TM. (thay mặt), KT. (ký thay), TL. (thừa lệnh), TUQ. (thừa ủy quyền), Q. (quyền).\nIn hoa, cỡ 13-14, đứng, đậm.\n\nHọ tên: In thường, đứng, đậm, canh giữa dưới quyền hạn.\nKhông ghi học hàm, học vị trước họ tên (trừ lực lượng vũ trang, giáo dục, y tế, khoa học).\n\nChữ ký số: PNG nền trong suốt, màu xanh, đặt giữa chức vụ và họ tên." },
            });

            var phuLuc2 = CreateAppendix("II", "Viết hoa trong văn bản hành chính",
                "I. VIẾT HOA VÌ PHÉP ĐẶT CÂU\nViết hoa chữ cái đầu âm tiết thứ nhất sau dấu chấm (.), hỏi (?), than (!) và khi xuống dòng.\n\n" +
                "II. VIẾT HOA DANH TỪ RIÊNG CHỈ TÊN NGƯỜI\n" +
                "• Tên người VN: Viết hoa chữ cái đầu tất cả âm tiết. VD: Nguyễn Ái Quốc\n" +
                "• Tên người nước ngoài phiên âm Hán-Việt: Như tên VN. VD: Mao Trạch Đông\n" +
                "• Phiên âm trực tiếp: Hoa chữ cái đầu âm tiết thứ nhất mỗi thành phần. VD: Vla-đi-mia I-lích Lê-nin\n\n" +
                "III. VIẾT HOA TÊN ĐỊA LÝ\n" +
                "• Đơn vị hành chính: Hoa chữ cái đầu tên riêng. VD: tỉnh Nam Định\n" +
                "• Đặc biệt: Thủ đô Hà Nội, Thành phố Hồ Chí Minh\n\n" +
                "IV. VIẾT HOA TÊN CƠ QUAN, TỔ CHỨC\n" +
                "• Hoa chữ cái đầu từ chỉ loại hình, chức năng. VD: Bộ Tài nguyên và Môi trường\n\n" +
                "V. CÁC TRƯỜNG HỢP KHÁC\n" +
                "• Nhân dân, Nhà nước: Luôn viết hoa\n" +
                "• Huân chương, danh hiệu: Hoa chữ cái đầu tên riêng + thứ hạng\n" +
                "• Chức vụ đi liền tên: Hoa chữ cái đầu. VD: Thủ tướng Chính phủ\n" +
                "• Đảng, Bác, Người (chỉ Hồ Chí Minh): Viết hoa\n" +
                "• Viện dẫn Điều, Khoản, Điểm: Viết hoa chữ cái đầu");

            var phuLuc3 = CreateAppendix("III", "Bảng chữ viết tắt tên loại VB & Mẫu trình bày",
                "BẢNG CHỮ VIẾT TẮT 29 LOẠI VĂN BẢN:\n\n" +
                "| STT | Tên loại văn bản | Viết tắt |\n" +
                "|-----|------------------|----------|\n" +
                "| 1   | Nghị quyết       | NQ       |\n" +
                "| 2   | Quyết định       | QĐ       |\n" +
                "| 3   | Chỉ thị          | CT       |\n" +
                "| 4   | Quy chế          | QC       |\n" +
                "| 5   | Quy định         | QyĐ     |\n" +
                "| 6   | Thông cáo        | TC       |\n" +
                "| 7   | Thông báo        | TB       |\n" +
                "| 8   | Hướng dẫn        | HD       |\n" +
                "| 9   | Chương trình     | CTr      |\n" +
                "| 10  | Kế hoạch         | KH       |\n" +
                "| 11  | Phương án        | PA       |\n" +
                "| 12  | Đề án           | ĐA       |\n" +
                "| 13  | Dự án           | DA       |\n" +
                "| 14  | Báo cáo         | BC       |\n" +
                "| 15  | Biên bản        | BB       |\n" +
                "| 16  | Tờ trình        | TTr      |\n" +
                "| 17  | Hợp đồng        | HĐ       |\n" +
                "| 18  | Công văn         | CV       |\n" +
                "| 19  | Công điện        | CĐ       |\n" +
                "| 20  | Bản ghi nhớ     | BGN      |\n" +
                "| 21  | Bản thỏa thuận  | BTT      |\n" +
                "| 22  | Giấy ủy quyền  | GUQ      |\n" +
                "| 23  | Giấy mời        | GM       |\n" +
                "| 24  | Giấy giới thiệu | GGT      |\n" +
                "| 25  | Giấy nghỉ phép  | GNP      |\n" +
                "| 26  | Phiếu gửi       | PG       |\n" +
                "| 27  | Phiếu chuyển    | PC       |\n" +
                "| 28  | Phiếu báo       | PB       |\n" +
                "| 29  | Thư công        | TC       |\n\n" +
                "MẪU TRÌNH BÀY:\n" +
                "• Mẫu 1.1: Nghị quyết (cá biệt)\n" +
                "• Mẫu 1.2: Quyết định quy định trực tiếp\n" +
                "• Mẫu 1.3: Quyết định quy định gián tiếp\n" +
                "• Mẫu 1.4: Văn bản có tên loại\n" +
                "• Mẫu 1.5: Công văn\n" +
                "• Mẫu 1.6: Bản sao y, sao lục, trích sao");

            var phuLuc4 = CreateAppendix("IV", "Mẫu về quản lý văn bản",
                "I. MẪU SỔ ĐĂNG KÝ VĂN BẢN ĐI\n" +
                "Tối thiểu 10 nội dung: Số/ký hiệu, ngày, tên loại, trích yếu, người ký, nơi nhận, đơn vị soạn thảo, ghi chú...\n\n" +
                "II. MẪU BÌ VĂN BẢN\n\n" +
                "III. MẪU SỔ GỬI VĂN BẢN ĐI BƯU ĐIỆN\nTối thiểu 6 nội dung.\n\n" +
                "IV. MẪU SỔ SỬ DỤNG BẢN LƯU\nTối thiểu 9 nội dung.\n\n" +
                "V. MẪU DẤU \"ĐẾN\": Hình chữ nhật 35mm x 50mm.\n\n" +
                "VI. MẪU SỔ ĐĂNG KÝ VĂN BẢN ĐẾN\nTối thiểu 10 nội dung: Ngày đến, số đến, tác giả, số/ký hiệu, ngày VB, trích yếu, đơn vị nhận...\n\n" +
                "VII. MẪU PHIẾU GIẢI QUYẾT VĂN BẢN ĐẾN\n3 phần: Ý kiến lãnh đạo, Ý kiến lãnh đạo đơn vị, Ý kiến đề xuất người giải quyết.\n\n" +
                "VIII. MẪU SỔ THEO DÕI GIẢI QUYẾT VĂN BẢN ĐẾN\nTối thiểu 7 nội dung.");

            var phuLuc5 = CreateAppendix("V", "Lập hồ sơ và nộp lưu hồ sơ",
                "I. XÂY DỰNG DANH MỤC HỒ SƠ\nGồm: Đề mục, số/ký hiệu, tiêu đề, thời hạn bảo quản, người lập.\n\n" +
                "II. MẪU DANH MỤC HỒ SƠ\n5 cột: STT, Đề mục/Số ký hiệu HS, Tiêu đề hồ sơ, Thời hạn bảo quản, Ghi chú.\n\n" +
                "III. MẪU MỤC LỤC HỒ SƠ NỘP LƯU\n7 cột: STT, Hồ sơ số, Tiêu đề hồ sơ, Ngày bắt đầu, Ngày kết thúc, Thời hạn BQ, Ghi chú.\n\n" +
                "IV. MẪU MỤC LỤC VĂN BẢN TRONG HỒ SƠ\n7 cột: STT, Số/ký hiệu, Ngày tháng, Tên loại và trích yếu, Tác giả, Tờ số, Ghi chú.\n\n" +
                "V. MẪU BIÊN BẢN GIAO NHẬN HỒ SƠ\nBên giao (đơn vị/cá nhân) - Bên nhận (Lưu trữ cơ quan).\nDanh mục tài liệu giao nhận.");

            var phuLuc6 = CreateAppendix("VI", "Yêu cầu đối với Hệ thống quản lý tài liệu điện tử",
                "PHẦN I — QUY ĐỊNH ĐỐI VỚI HỆ THỐNG:\n\n" +
                "I. NGUYÊN TẮC XÂY DỰNG (5 nguyên tắc)\n" +
                "• Đáp ứng yêu cầu quản lý, sử dụng VB\n" +
                "• Phù hợp quy định pháp luật\n" +
                "• Kết nối, liên thông\n" +
                "• Bảo mật thông tin\n" +
                "• Dễ sử dụng\n\n" +
                "II. YÊU CẦU CHUNG THIẾT KẾ (8 yêu cầu)\n\n" +
                "III. YÊU CẦU CHỨC NĂNG (8 nhóm):\n" +
                "1. Tạo lập, soạn thảo VB\n" +
                "2. Kết nối, liên thông\n" +
                "3. An ninh, bảo mật\n" +
                "4. Quản lý hồ sơ\n" +
                "5. Bảo quản tài liệu\n" +
                "6. Thống kê, báo cáo\n" +
                "7. Quản lý dữ liệu đặc tả\n" +
                "8. Thu hồi VB\n\n" +
                "IV. YÊU CẦU QUẢN TRỊ HỆ THỐNG\n\n" +
                "V. THÔNG TIN ĐẦU RA (6 loại sổ/báo cáo)\n\n" +
                "PHẦN II — CHUẨN THÔNG TIN ĐẦU VÀO:\n\n" +
                "I. VĂN BẢN ĐI (16 trường):\n" +
                "CodeNumber, CodeNotation, IssuedDate, Subject, TypeCode, SteeringType, " +
                "OrganizationCode, SignerFullName, SignerPosition, DueDate, Content, " +
                "SecurityLevel, UrgencyLevel, AppendixList, RecipientList, StatusCode\n\n" +
                "II. VĂN BẢN ĐẾN (18 trường): Thêm ArrivalDate, ArrivalNumber, TraceHeaderList...\n\n" +
                "III. HỒ SƠ (11 trường): FileCode, Title, Maintenance, Creator, DateStart...");

            root.Children.AddRange(new[] { chuong1, chuong2, chuong3, chuong4, chuong5, chuong6 });
            root.Children.AddRange(new[] { phuLuc1, phuLuc2, phuLuc3, phuLuc4, phuLuc5, phuLuc6 });

            return new List<LegalNode> { root };
        }

        private static LegalNode CreateArticle(int number, string title, string content, List<string>? tags = null)
        {
            return new LegalNode
            {
                Id = $"dieu-{number}",
                Title = $"Điều {number}. {title}",
                Content = content,
                NodeType = LegalNodeType.Article,
                FeatureTags = tags ?? new(),
                Icon = "TextBoxOutline"
            };
        }

        private static LegalNode CreateAppendix(string roman, string title, string content)
        {
            return new LegalNode
            {
                Id = $"phu-luc-{roman}",
                Title = $"Phụ lục {roman} — {title}",
                Content = content,
                NodeType = LegalNodeType.Appendix,
                Icon = "FileDocumentMultiple"
            };
        }

        #endregion
    }
}
