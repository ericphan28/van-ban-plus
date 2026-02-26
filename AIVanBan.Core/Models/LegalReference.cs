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
                // Phần I — Thể thức và kỹ thuật trình bày VB hành chính
                new LegalNode { Id = "pl1-quy-dinh-chung", Title = "I. Quy định chung", NodeType = LegalNodeType.SubSection,
                    Content = "1. Khổ giấy: A4 (210 mm × 297 mm).\n" +
                        "2. Kiểu trình bày: Theo chiều dài khổ A4. Nội dung có bảng biểu → có thể theo chiều rộng.\n" +
                        "3. Định lề trang:\n   • Trên, dưới: 20 – 25 mm\n   • Trái: 30 – 35 mm\n   • Phải: 15 – 20 mm\n" +
                        "4. Phông chữ: Times New Roman, bộ mã Unicode TCVN 6909:2001, màu đen.\n" +
                        "5. Cỡ chữ và kiểu chữ: Theo quy định cho từng thành phần thể thức.\n" +
                        "6. Vị trí trình bày: Theo Mục IV Phần I Phụ lục này.\n" +
                        "7. Số trang: Chữ số Ả Rập, cỡ 13-14, đứng, canh giữa lề trên, không hiển thị trang 1." },
                new LegalNode { Id = "pl1-quoc-hieu", Title = "II.1. Quốc hiệu và Tiêu ngữ", NodeType = LegalNodeType.SubSection,
                    Content = "| Thành phần | Loại chữ | Cỡ chữ | Kiểu chữ | Ví dụ |\n" +
                        "|------------|----------|---------|----------|-------|\n" +
                        "| Quốc hiệu | In hoa | 12-13 | Đứng, đậm | CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM |\n" +
                        "| Tiêu ngữ | In thường | 13-14 | Đứng, đậm | Độc lập - Tự do - Hạnh phúc |\n\n" +
                        "QUY CÁCH TRÌNH BÀY:\n" +
                        "• Quốc hiệu: Phía trên cùng, bên phải trang đầu tiên.\n" +
                        "• Tiêu ngữ: Canh giữa dưới Quốc hiệu, chữ cái đầu viết hoa, giữa các cụm từ có gạch nối (-).\n" +
                        "• Phía dưới Tiêu ngữ: đường kẻ ngang nét liền, dài bằng dòng chữ.\n" +
                        "• Hai dòng cách nhau dòng đơn.\n\nVị trí: Ô số 1, Mục IV." },
                new LegalNode { Id = "pl1-ten-co-quan", Title = "II.2. Tên cơ quan ban hành", NodeType = LegalNodeType.SubSection,
                    Content = "• Tên CQ chủ quản: In hoa, cỡ 12-13, đứng.\n" +
                        "• Tên CQ ban hành: In hoa, cỡ 12-13, đứng, đậm, canh giữa dưới tên CQ chủ quản.\n" +
                        "• Phía dưới: đường kẻ ngang nét liền, dài 1/3 – 1/2 dòng chữ.\n" +
                        "• Đối với địa phương: thêm tên tỉnh/huyện/xã nơi đóng trụ sở.\n\nVị trí: Ô số 2, Mục IV." },
                new LegalNode { Id = "pl1-so-ky-hieu", Title = "II.3. Số, ký hiệu văn bản", NodeType = LegalNodeType.SubSection,
                    Content = "• Số: Chữ số Ả Rập, số < 10 thêm 0 phía trước (01, 02...).\n" +
                        "• Ký hiệu VB có tên loại: Viết tắt tên loại + tên CQ. VD: QĐ-UBND\n" +
                        "• Ký hiệu Công văn: Viết tắt tên CQ + đơn vị soạn thảo. VD: UBND-VP\n" +
                        "• Giữa số và ký hiệu: dấu gạch chéo (/). Giữa nhóm viết tắt: gạch nối (-).\n" +
                        "• Cỡ chữ 13, đứng, canh giữa dưới tên CQ.\n" +
                        "• VD: Số: 30/NĐ-CP, Số: 05/BNV-VP\n\nVị trí: Ô số 3, Mục IV." },
                new LegalNode { Id = "pl1-dia-danh", Title = "II.4. Địa danh và thời gian", NodeType = LegalNodeType.SubSection,
                    Content = "• Địa danh: Tên chính thức đơn vị hành chính nơi CQ đóng trụ sở.\n" +
                        "• Thời gian: Ngày, tháng, năm bằng chữ số Ả Rập. Ngày < 10, tháng 1-2: thêm số 0.\n" +
                        "• In thường, cỡ 13-14, kiểu nghiêng, cùng dòng với số/ký hiệu.\n" +
                        "• Chữ cái đầu địa danh viết hoa, sau có dấu phẩy (,).\n" +
                        "• VD: Hà Nội, ngày 05 tháng 3 năm 2020\n\nVị trí: Ô số 4, Mục IV." },
                new LegalNode { Id = "pl1-ten-loai", Title = "II.5. Tên loại và trích yếu", NodeType = LegalNodeType.SubSection,
                    Content = "• Tên loại VB: In hoa, cỡ 13-14, đứng, đậm, canh giữa.\n" +
                        "• Trích yếu: In thường, cỡ 13-14, đứng, đậm, ngay dưới tên loại, canh giữa.\n" +
                        "• Bên dưới: đường kẻ ngang nét liền, dài 1/3 – 1/2 dòng chữ.\n" +
                        "• Công văn: Trích yếu sau \"V/v\", in thường, cỡ 12-13, canh giữa dưới số/ký hiệu.\n\n" +
                        "Vị trí: Ô số 5a (VB có tên loại), 5b (Công văn)." },
                new LegalNode { Id = "pl1-noi-dung", Title = "II.6. Nội dung văn bản", NodeType = LegalNodeType.SubSection,
                    Content = "• In thường, canh đều 2 lề, cỡ 13-14, đứng.\n" +
                        "• Lùi đầu dòng 1 cm hoặc 1,27 cm. Khoảng cách đoạn ≥ 6pt. Dòng: đơn → 1,5 lines.\n\n" +
                        "BỐ CỤC: Phần → Chương → Mục → Tiểu mục → Điều → Khoản → Điểm\n" +
                        "• Phần/Chương: Số La Mã, đứng đậm, canh giữa. Tiêu đề in hoa, đậm.\n" +
                        "• Điều: Số Ả Rập + dấu chấm, lùi đầu dòng, đứng đậm.\n" +
                        "• Khoản: Số Ả Rập + dấu chấm. Điểm: Chữ cái + dấu đóng ngoặc.\n\n" +
                        "Căn cứ ban hành: Chữ nghiêng, cỡ 13-14, cuối dòng dấu (;), dòng cuối dấu (.)\n\nVị trí: Ô số 6." },
                new LegalNode { Id = "pl1-chu-ky", Title = "II.7. Chữ ký, quyền hạn", NodeType = LegalNodeType.SubSection,
                    Content = "Quyền hạn ký:\n• TM. — Thay mặt tập thể\n• Q. — Giao quyền cấp trưởng\n" +
                        "• KT. — Ký thay người đứng đầu\n• TL. — Thừa lệnh\n• TUQ. — Thừa ủy quyền\n\n" +
                        "Trình bày: In hoa, cỡ 13-14, đứng, đậm.\n" +
                        "Họ tên: In thường, đứng, đậm, canh giữa. Không ghi học hàm, học vị.\n" +
                        "Chữ ký số: PNG nền trong suốt, màu xanh, đặt giữa chức vụ và họ tên.\n\nVị trí: Ô số 7a, 7b, 7c." },
                new LegalNode { Id = "pl1-dau-noi-nhan", Title = "II.8–9. Dấu, nơi nhận", NodeType = LegalNodeType.SubSection,
                    Content = "DẤU, CHỮ KÝ SỐ CƠ QUAN (Ô số 8):\n" +
                        "• Dấu đỏ, kích thước thực, PNG nền trong suốt, trùm 1/3 chữ ký bên trái.\n" +
                        "• VB kèm theo cùng tệp: không ký số phụ lục. Khác tệp: ký số góc trên phải.\n\n" +
                        "NƠI NHẬN (Ô số 9a, 9b):\n" +
                        "• \"Kính gửi:\" (9a): Cỡ 13-14, đứng. 1 nơi: cùng dòng. Nhiều nơi: gạch đầu dòng.\n" +
                        "• \"Nơi nhận:\" (9b): Cỡ 12, nghiêng đậm. Liệt kê: cỡ 11, đứng.\n" +
                        "• Cuối: \"- Lưu: VT, [đơn vị soạn thảo].\"" },
                new LegalNode { Id = "pl1-thanh-phan-khac", Title = "III. Thành phần thể thức khác", NodeType = LegalNodeType.SubSection,
                    Content = "1. Phụ lục kèm theo: Số La Mã, \"Phụ lục\" cỡ 14, đứng, đậm. Tên in hoa.\n\n" +
                        "2. Dấu chỉ độ mật: TUYỆT MẬT, TỐI MẬT, MẬT — theo luật BVBMNN. Ô 10a.\n\n" +
                        "3. Dấu chỉ mức độ khẩn:\n" +
                        "   HOẢ TỐC (30×8mm), THƯỢNG KHẨN (40×8mm), KHẨN (20×8mm)\n" +
                        "   In hoa, đậm, khung chữ nhật viền đơn, mực đỏ. Ô 10b.\n\n" +
                        "4. Chỉ dẫn lưu hành: \"XEM XONG TRẢ LẠI\", \"LƯU HÀNH NỘI BỘ\". Ô 11.\n\n" +
                        "5. Ký hiệu người soạn thảo & số bản: Cỡ 11, in hoa. VD: PL.(300). Ô 12.\n\n" +
                        "6. Địa chỉ CQ, ĐT, Fax, Email, Website: Cỡ 11-12, đứng. Ô 13." },
                new LegalNode { Id = "pl1-so-do", Title = "IV. Sơ đồ bố trí 14 ô trên A4", NodeType = LegalNodeType.SubSection,
                    Content = "Vị trí các thành phần thể thức trên khổ A4:\n\n" +
                        "| Ô số | Thành phần thể thức |\n" +
                        "|------|---------------------|\n" +
                        "| 1 | Quốc hiệu và Tiêu ngữ |\n" +
                        "| 2 | Tên cơ quan, tổ chức ban hành văn bản |\n" +
                        "| 3 | Số, ký hiệu của văn bản |\n" +
                        "| 4 | Địa danh và thời gian ban hành |\n" +
                        "| 5a | Tên loại và trích yếu nội dung |\n" +
                        "| 5b | Trích yếu nội dung công văn |\n" +
                        "| 6 | Nội dung văn bản |\n" +
                        "| 7a, 7b, 7c | Chức vụ, họ tên và chữ ký người có thẩm quyền |\n" +
                        "| 8 | Dấu, Chữ ký số của cơ quan, tổ chức |\n" +
                        "| 9a, 9b | Nơi nhận |\n" +
                        "| 10a | Dấu chỉ độ mật |\n" +
                        "| 10b | Dấu chỉ mức độ khẩn |\n" +
                        "| 11 | Chỉ dẫn về phạm vi lưu hành |\n" +
                        "| 12 | Ký hiệu người soạn thảo và số lượng bản phát hành |\n" +
                        "| 13 | Địa chỉ, ĐT, Fax, Email, Website |\n" +
                        "| 14 | Chữ ký số CQ cho bản sao điện tử |" },
                new LegalNode { Id = "pl1-mau-chu", Title = "V. Mẫu chữ và chi tiết trình bày", NodeType = LegalNodeType.SubSection,
                    Content = "BẢNG MẪU CHỮ VÀ CHI TIẾT TRÌNH BÀY THỂ THỨC VĂN BẢN HÀNH CHÍNH:\n" +
                        "(Theo Phụ lục I, NĐ 30/2020/NĐ-CP)\n\n" +
                        "| STT | Thành phần thể thức | Loại chữ | Cỡ chữ | Kiểu chữ |\n" +
                        "|-----|---------------------|----------|---------|----------|\n" +
                        "| 1 | Quốc hiệu | In hoa | 12-13 | Đứng, đậm |\n" +
                        "|   | Tiêu ngữ | In thường | 13-14 | Đứng, đậm |\n" +
                        "| 2 | Tên CQ chủ quản trực tiếp | In hoa | 12-13 | Đứng |\n" +
                        "|   | Tên CQ ban hành VB | In hoa | 12-13 | Đứng, đậm |\n" +
                        "| 3 | Số, ký hiệu VB | In thường | 13 | Đứng |\n" +
                        "| 4 | Địa danh và thời gian ban hành | In thường | 13-14 | Nghiêng |\n" +
                        "| 5a | Tên loại VB (có tên loại) | In hoa | 13-14 | Đứng, đậm |\n" +
                        "|   | Trích yếu nội dung | In thường | 13-14 | Đứng, đậm |\n" +
                        "| 5b | Trích yếu nội dung (công văn) | In thường | 12-13 | Đứng |\n" +
                        "| 6 | Nội dung văn bản | In thường | 13-14 | Đứng |\n" +
                        "|   | Phần, Chương (số thứ tự) | In thường | 13-14 | Đứng, đậm |\n" +
                        "|   | Tiêu đề phần, chương | In hoa | 13-14 | Đứng, đậm |\n" +
                        "|   | Mục (số thứ tự) | In thường | 13-14 | Đứng, đậm |\n" +
                        "|   | Tiêu đề mục | In hoa | 13-14 | Đứng, đậm |\n" +
                        "|   | Tiểu mục (số thứ tự) | In thường | 13-14 | Đứng, đậm |\n" +
                        "|   | Tiêu đề tiểu mục | In hoa | 13-14 | Đứng, đậm |\n" +
                        "|   | Điều | In thường | 13-14 | Đứng, đậm |\n" +
                        "|   | Khoản | In thường | 13-14 | Đứng |\n" +
                        "|   | Điểm | In thường | 13-14 | Đứng |\n" +
                        "| 7 | Quyền hạn của người ký | In hoa | 13-14 | Đứng, đậm |\n" +
                        "|   | Chức vụ của người ký | In hoa | 13-14 | Đứng, đậm |\n" +
                        "|   | Họ tên của người ký | In thường | 13-14 | Đứng, đậm |\n" +
                        "| 8a | Từ \"Kính gửi\" và tên nơi nhận | In thường | 13-14 | Đứng |\n" +
                        "| 8b | Từ \"Nơi nhận\" | In thường | 12 | Nghiêng, đậm |\n" +
                        "|   | Tên CQ, tổ chức, cá nhân nhận VB | In thường | 11 | Đứng |\n" +
                        "| 9 | Từ \"Phụ lục\" và số thứ tự | In thường | 14 | Đứng, đậm |\n" +
                        "|   | Tiêu đề phụ lục | In hoa | 13-14 | Đứng, đậm |\n" +
                        "| 10 | Dấu chỉ mức độ khẩn | In hoa | 13-14 | Đứng, đậm |\n" +
                        "| 11 | Ký hiệu người soạn thảo và số bản | In thường | 11 | Đứng |\n" +
                        "| 12 | Địa chỉ CQ, ĐT, Fax, Email, Website | In thường | 11-12 | Đứng |\n" +
                        "| 13 | Chỉ dẫn phạm vi lưu hành | In hoa | 13-14 | Đứng, đậm |\n" +
                        "| 14 | Số trang | In thường | 13-14 | Đứng |" },
                // Phần II — Bản sao văn bản
                new LegalNode { Id = "pl1-ban-sao", Title = "Phần II. Bản sao văn bản", NodeType = LegalNodeType.SubSection,
                    Content = "BẢN SAO SANG ĐỊNH DẠNG ĐIỆN TỬ:\n" +
                        "• Hình thức: \"SAO Y\", \"SAO LỤC\", \"TRÍCH SAO\"\n" +
                        "• Tiêu chuẩn: PDF ≥ 1.4, ảnh màu, ≥ 200dpi, tỷ lệ 100%\n" +
                        "• Chữ ký số: Góc trên bên phải, không hiển thị hình ảnh, Times New Roman cỡ 10\n\n" +
                        "BẢN SAO SANG ĐỊNH DẠNG GIẤY:\n" +
                        "• Gồm: Hình thức sao, tên CQ sao, số/ký hiệu, địa danh, ngày tháng, chữ ký, dấu, nơi nhận\n" +
                        "• Trình bày: Trên cùng tờ A4, sau phần cuối VB, dưới đường kẻ ngang nét liền\n" +
                        "• \"SAO Y\"/\"SAO LỤC\"/\"TRÍCH SAO\": In hoa, cỡ 13-14, đứng, đậm" },
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
            phuLuc2.Children.AddRange(new[]
            {
                new LegalNode { Id = "pl2-phep-dat-cau", Title = "I. Viết hoa vì phép đặt câu", NodeType = LegalNodeType.SubSection,
                    Content = "Viết hoa chữ cái đầu âm tiết thứ nhất của một câu hoàn chỉnh:\n• Sau dấu chấm câu (.)\n• Sau dấu chấm hỏi (?)\n• Sau dấu chấm than (!)\n• Khi xuống dòng" },
                new LegalNode { Id = "pl2-ten-nguoi", Title = "II. Viết hoa tên người", NodeType = LegalNodeType.SubSection,
                    Content = "1. Tên người Việt Nam:\n• Tên thông thường: Viết hoa chữ cái đầu tất cả âm tiết. VD: Nguyễn Ái Quốc, Trần Phú\n• Tên hiệu, nhân vật lịch sử: Viết hoa tất cả. VD: Bà Triệu, Bác Hồ, Ông Gióng\n\n2. Tên người nước ngoài phiên âm:\n• Hán-Việt: Như tên VN. VD: Kim Nhật Thành, Mao Trạch Đông\n• Phiên âm trực tiếp: Hoa chữ cái đầu âm tiết thứ nhất mỗi thành phần. VD: Vla-đi-mia I-lích Lê-nin" },
                new LegalNode { Id = "pl2-ten-dia-ly", Title = "III. Viết hoa tên địa lý", NodeType = LegalNodeType.SubSection,
                    Content = "1. Tên địa lý Việt Nam:\n• Đơn vị hành chính: Hoa chữ cái đầu tên riêng, không gạch nối. VD: thành phố Thái Nguyên, tỉnh Nam Định\n• Đặt theo số/tên người: Hoa cả danh từ chung. VD: Quận 1, Phường Điện Biên Phủ\n• Đặc biệt: Thủ đô Hà Nội, Thành phố Hồ Chí Minh\n• Địa hình + tên riêng (1 âm tiết) → tên riêng: Hoa tất cả. VD: Cửa Lò, Vũng Tàu\n• Vùng/miền: Hoa tất cả. VD: Tây Bắc, Bắc Bộ\n\n2. Tên địa lý nước ngoài:\n• Hán-Việt: Như tên VN. VD: Bắc Kinh\n• Phiên âm trực tiếp: Như tên người nước ngoài. VD: Mát-xcơ-va" },
                new LegalNode { Id = "pl2-ten-co-quan", Title = "IV. Viết hoa tên cơ quan, tổ chức", NodeType = LegalNodeType.SubSection,
                    Content = "1. CQ, tổ chức Việt Nam:\n• Viết hoa chữ cái đầu từ chỉ loại hình, chức năng, lĩnh vực hoạt động.\n• VD: Ban Chỉ đạo trung ương về Phòng chống tham nhũng\n• VD: Bộ Tài nguyên và Môi trường, Sở Tài chính\n• VD: Hội đồng nhân dân tỉnh Sơn La\n\n2. CQ, tổ chức nước ngoài:\n• Dịch nghĩa: Như CQ VN. VD: Liên hợp quốc (UN), Tổ chức Y tế thế giới (WHO)\n• Viết tắt: In hoa nguyên ngữ. VD: WTO, ASEAN, UNESCO" },
                new LegalNode { Id = "pl2-truong-hop-khac", Title = "V. Các trường hợp khác", NodeType = LegalNodeType.SubSection,
                    Content = "1. Danh từ đặc biệt: Nhân dân, Nhà nước — luôn viết hoa.\n2. Huân chương, danh hiệu: Hoa chữ cái đầu tên riêng + thứ hạng. VD: Huân chương Sao vàng, Nghệ sĩ Nhân dân\n3. Chức vụ đi liền tên: Hoa chữ cái đầu. VD: Chủ tịch Quốc hội, Thủ tướng Chính phủ\n4. Danh từ riêng hóa: Bác, Người (Hồ Chí Minh), Đảng (Đảng CSVN)\n5. Ngày lễ: Hoa chữ cái đầu. VD: ngày Quốc khánh 2-9\n6. Tên loại VB cụ thể: Hoa chữ cái đầu. VD: Bộ luật Hình sự, Luật Tổ chức Quốc hội\n7. Viện dẫn Phần, Chương, Mục, Điều, Khoản, Điểm: Viết hoa chữ cái đầu\n8. Năm âm lịch: Hoa tất cả. VD: Kỷ Tỵ, Mậu Tuất\n9. Ngày tết: tết Nguyên đán, tết Trung thu. Viết hoa 'Tết' khi thay cho tết Nguyên đán\n10. Sự kiện lịch sử: Hoa tên sự kiện. VD: Cách mạng tháng Tám" },
            });

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
            phuLuc3.Children.AddRange(new[]
            {
                new LegalNode { Id = "pl3-bang-viet-tat", Title = "I. Bảng chữ viết tắt 29 loại VB", NodeType = LegalNodeType.SubSection,
                    Content = "BẢNG CHỮ VIẾT TẮT TÊN LOẠI VĂN BẢN HÀNH CHÍNH:\n\n" +
                        "| STT | Tên loại văn bản | Viết tắt |\n" +
                        "|-----|------------------|----------|\n" +
                        "| 1 | Nghị quyết (cá biệt) | NQ |\n" +
                        "| 2 | Quyết định (cá biệt) | QĐ |\n" +
                        "| 3 | Chỉ thị | CT |\n" +
                        "| 4 | Quy chế | QC |\n" +
                        "| 5 | Quy định | QyĐ |\n" +
                        "| 6 | Thông cáo | TC |\n" +
                        "| 7 | Thông báo | TB |\n" +
                        "| 8 | Hướng dẫn | HD |\n" +
                        "| 9 | Chương trình | CTr |\n" +
                        "| 10 | Kế hoạch | KH |\n" +
                        "| 11 | Phương án | PA |\n" +
                        "| 12 | Đề án | ĐA |\n" +
                        "| 13 | Dự án | DA |\n" +
                        "| 14 | Báo cáo | BC |\n" +
                        "| 15 | Biên bản | BB |\n" +
                        "| 16 | Tờ trình | TTr |\n" +
                        "| 17 | Hợp đồng | HĐ |\n" +
                        "| 18 | Công điện | CĐ |\n" +
                        "| 19 | Bản ghi nhớ | BGN |\n" +
                        "| 20 | Bản thỏa thuận | BTT |\n" +
                        "| 21 | Giấy ủy quyền | GUQ |\n" +
                        "| 22 | Giấy mời | GM |\n" +
                        "| 23 | Giấy giới thiệu | GGT |\n" +
                        "| 24 | Giấy nghỉ phép | GNP |\n" +
                        "| 25 | Phiếu gửi | PG |\n" +
                        "| 26 | Phiếu chuyển | PC |\n" +
                        "| 27 | Phiếu báo | PB |\n\n" +
                        "BẢN SAO VĂN BẢN:\n\n" +
                        "| Hình thức sao | Viết tắt |\n" +
                        "|--------------|----------|\n" +
                        "| Bản sao y | SY |\n" +
                        "| Trích sao | TrS |\n" +
                        "| Sao lục | SL |" },
                new LegalNode { Id = "pl3-mau-van-ban", Title = "II. Mẫu trình bày VB hành chính", NodeType = LegalNodeType.SubSection,
                    Content = "DANH MỤC 10 MẪU TRÌNH BÀY VĂN BẢN:\n\n" +
                        "| Mẫu số | Tên văn bản |\n" +
                        "|---------|------------|\n" +
                        "| Mẫu 1.1 | Nghị quyết (cá biệt) |\n" +
                        "| Mẫu 1.2 | Quyết định quy định trực tiếp |\n" +
                        "| Mẫu 1.3 | Quyết định quy định gián tiếp (ban hành/phê duyệt VB khác) |\n" +
                        "| Mẫu 1.4 | Văn bản có tên loại (chỉ thị, quy chế, thông báo, kế hoạch, báo cáo, tờ trình...) |\n" +
                        "| Mẫu 1.5 | Công văn |\n" +
                        "| Mẫu 1.6 | Công điện |\n" +
                        "| Mẫu 1.7 | Giấy mời |\n" +
                        "| Mẫu 1.8 | Giấy giới thiệu |\n" +
                        "| Mẫu 1.9 | Biên bản |\n" +
                        "| Mẫu 1.10 | Giấy nghỉ phép |\n\n" +
                        "Mỗi mẫu quy định vị trí các thành phần thể thức: Quốc hiệu, tên CQ, số/ký hiệu, địa danh, trích yếu, nội dung, chữ ký, nơi nhận." },
                new LegalNode { Id = "pl3-mau-phu-luc-ban-sao", Title = "III. Mẫu phụ lục & bản sao", NodeType = LegalNodeType.SubSection,
                    Content = "MẪU TRÌNH BÀY PHỤ LỤC VĂN BẢN:\n" +
                        "• Mẫu 2.1: Phụ lục VB hành chính giấy — gồm số thứ tự, tiêu đề, thông tin kèm theo VB\n" +
                        "• Mẫu 2.2: Phụ lục VB hành chính điện tử — áp dụng khi phụ lục không cùng tệp tin\n\n" +
                        "MẪU TRÌNH BÀY BẢN SAO VĂN BẢN:\n" +
                        "• Mẫu 3.1: Bản sao sang định dạng giấy — trình bày sau phần cuối VB cần sao\n" +
                        "• Mẫu 3.2: Bản sao sang định dạng điện tử — chữ ký số CQ, không hiển thị hình ảnh" },
            });

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
            phuLuc4.Children.AddRange(new[]
            {
                new LegalNode { Id = "pl4-so-vb-di", Title = "I. Mẫu sổ đăng ký VB đi", NodeType = LegalNodeType.SubSection,
                    Content = "SỔ ĐĂNG KÝ VĂN BẢN ĐI — tối thiểu 10 cột nội dung:\n\n" +
                        "| Số, ký hiệu VB | Ngày tháng VB | Tên loại và trích yếu ND | Người ký | Nơi nhận VB | Đơn vị, người nhận bản lưu | Số lượng bản | Ngày chuyển | Ký nhận | Ghi chú |\n" +
                        "|---|---|---|---|---|---|---|---|---|---|\n" +
                        "| (1) | (2) | (3) | (4) | (5) | (6) | (7) | (8) | (9) | (10) |" },
                new LegalNode { Id = "pl4-bi-vb-buu-dien", Title = "II. Mẫu bì VB & Sổ gửi bưu điện", NodeType = LegalNodeType.SubSection,
                    Content = "MẪU BÌ VĂN BẢN: Theo quy cách chuẩn.\n\n" +
                        "SỔ GỬI VB ĐI BƯU ĐIỆN — tối thiểu 6 nội dung:\n\n" +
                        "| Ngày chuyển | Số, ký hiệu VB | Nơi nhận VB | Số lượng bì | Ký nhận và dấu bưu điện | Ghi chú |\n" +
                        "|---|---|---|---|---|---|\n" +
                        "| (1) | (2) | (3) | (4) | (5) | (6) |\n\n" +
                        "SỔ SỬ DỤNG BẢN LƯU — tối thiểu 9 nội dung:\n\n" +
                        "| Ngày tháng | Họ tên người SD | Số, ký hiệu ngày tháng VB | Tên loại và trích yếu ND VB | Số và ký hiệu HS | Ký nhận | Ngày trả | Người cho phép SD | Ghi chú |\n" +
                        "|---|---|---|---|---|---|---|---|---|\n" +
                        "| (1) | (2) | (3) | (4) | (5) | (6) | (7) | (8) | (9) |" },
                new LegalNode { Id = "pl4-dau-den-so-den", Title = "III. Dấu \"ĐẾN\" & Sổ đăng ký VB đến", NodeType = LegalNodeType.SubSection,
                    Content = "MẪU DẤU \"ĐẾN\":\n" +
                        "• Được khắc sẵn, hình chữ nhật, kích thước 35 mm × 50 mm\n" +
                        "• Gồm: Tên CQ/tổ chức, Số đến, Ngày đến, Chuyển, Số và ký hiệu HS\n\n" +
                        "SỔ ĐĂNG KÝ VĂN BẢN ĐẾN — tối thiểu 10 cột nội dung:\n\n" +
                        "| Ngày đến | Số đến | Tác giả | Số, ký hiệu VB | Ngày tháng VB | Tên loại và trích yếu ND VB | Đơn vị hoặc người nhận | Ngày chuyển | Ký nhận | Ghi chú |\n" +
                        "|---|---|---|---|---|---|---|---|---|---|\n" +
                        "| (1) | (2) | (3) | (4) | (5) | (6) | (7) | (8) | (9) | (10) |" },
                new LegalNode { Id = "pl4-phieu-theo-doi", Title = "IV. Phiếu giải quyết & Sổ theo dõi VB đến", NodeType = LegalNodeType.SubSection,
                    Content = "MẪU PHIẾU GIẢI QUYẾT VB ĐẾN — 3 phần:\n" +
                        "1. Ý kiến của lãnh đạo CQ, tổ chức:\n" +
                        "   • Giao đơn vị, cá nhân chủ trì\n" +
                        "   • Giao các đơn vị, cá nhân tham gia phối hợp giải quyết VB đến (nếu có)\n" +
                        "   • Thời hạn giải quyết đối với mỗi đơn vị, cá nhân (nếu có)\n" +
                        "   • Ngày tháng cho ý kiến phân phối, giải quyết\n" +
                        "2. Ý kiến của lãnh đạo đơn vị:\n" +
                        "   • Giao cho cá nhân; thời hạn giải quyết đối với cá nhân (nếu có)\n" +
                        "   • Ngày, tháng, năm cho ý kiến\n" +
                        "3. Ý kiến đề xuất của người giải quyết:\n" +
                        "   • Ý kiến đề xuất giải quyết VB đến của cá nhân\n" +
                        "   • Ngày, tháng, năm đề xuất ý kiến\n\n" +
                        "MẪU SỔ THEO DÕI GIẢI QUYẾT VB ĐẾN — tối thiểu 7 cột:\n\n" +
                        "| Số đến | Tên loại, số, ký hiệu, ngày, tháng và tên CQ ban hành VB | Đơn vị hoặc người nhận | Thời hạn giải quyết | Tiến độ giải quyết | Số, ký hiệu VB trả lời | Ghi chú |\n" +
                        "|---|---|---|---|---|---|---|\n" +
                        "| (1) | (2) | (3) | (4) | (5) | (6) | (7) |" },
            });

            var phuLuc5 = CreateAppendix("V", "Lập hồ sơ và nộp lưu hồ sơ",
                "I. XÂY DỰNG DANH MỤC HỒ SƠ\nGồm: Đề mục, số/ký hiệu, tiêu đề, thời hạn bảo quản, người lập.\n\n" +
                "II. MẪU DANH MỤC HỒ SƠ\n5 cột: STT, Đề mục/Số ký hiệu HS, Tiêu đề hồ sơ, Thời hạn bảo quản, Ghi chú.\n\n" +
                "III. MẪU MỤC LỤC HỒ SƠ NỘP LƯU\n7 cột: STT, Hồ sơ số, Tiêu đề hồ sơ, Ngày bắt đầu, Ngày kết thúc, Thời hạn BQ, Ghi chú.\n\n" +
                "IV. MẪU MỤC LỤC VĂN BẢN TRONG HỒ SƠ\n7 cột: STT, Số/ký hiệu, Ngày tháng, Tên loại và trích yếu, Tác giả, Tờ số, Ghi chú.\n\n" +
                "V. MẪU BIÊN BẢN GIAO NHẬN HỒ SƠ\nBên giao (đơn vị/cá nhân) - Bên nhận (Lưu trữ cơ quan).\nDanh mục tài liệu giao nhận.");
            phuLuc5.Children.AddRange(new[]
            {
                new LegalNode { Id = "pl5-danh-muc-hs", Title = "I. Xây dựng danh mục hồ sơ", NodeType = LegalNodeType.SubSection,
                    Content = "Danh mục hồ sơ gồm các thành phần:\n" +
                        "1) Đề mục: Theo cơ cấu tổ chức hoặc lĩnh vực hoạt động. Đề mục lớn đánh số La Mã, đề mục nhỏ đánh số Ả Rập.\n" +
                        "2) Số, ký hiệu hồ sơ: Số thứ tự (Ả Rập) + ký hiệu viết tắt đề mục lớn. VD: 01.TCCB\n" +
                        "3) Tiêu đề hồ sơ: Ngắn gọn, rõ ràng, khái quát nội dung VB/tài liệu.\n" +
                        "4) Thời hạn bảo quản: Vĩnh viễn hoặc số năm cụ thể.\n" +
                        "5) Người lập hồ sơ.\n\n" +
                        "MẪU DANH MỤC HỒ SƠ (5 cột):\n\n" +
                        "| Số và ký hiệu hồ sơ | Tên đề mục và tiêu đề hồ sơ | Thời hạn bảo quản | Người lập hồ sơ | Ghi chú |\n" +
                        "|---|---|---|---|---|\n" +
                        "| (1) | (2) | (3) | (4) | (5) |" },
                new LegalNode { Id = "pl5-muc-luc-hs", Title = "II. Mẫu mục lục hồ sơ nộp lưu", NodeType = LegalNodeType.SubSection,
                    Content = "MẪU MỤC LỤC HỒ SƠ, TÀI LIỆU NỘP LƯU (7 cột):\n\n" +
                        "| STT | Số, ký hiệu hồ sơ | Tiêu đề hồ sơ | Thời gian tài liệu | Thời hạn bảo quản | Số tờ/trang | Ghi chú |\n" +
                        "|---|---|---|---|---|---|---|\n" +
                        "| (1) | (2) | (3) | (4) | (5) | (6) | (7) |\n\n" +
                        "MẪU MỤC LỤC VĂN BẢN TRONG HỒ SƠ (7 cột):\n\n" +
                        "| STT | Số, ký hiệu VB | Ngày tháng năm VB | Tên loại và trích yếu ND | Tác giả VB | Tờ số / Trang số | Ghi chú |\n" +
                        "|---|---|---|---|---|---|---|\n" +
                        "| (1) | (2) | (3) | (4) | (5) | (6) | (7) |" },
                new LegalNode { Id = "pl5-bien-ban-giao-nhan", Title = "III. Biên bản giao nhận hồ sơ", NodeType = LegalNodeType.SubSection,
                    Content = "MẪU BIÊN BẢN GIAO NHẬN HỒ SƠ, TÀI LIỆU:\n" +
                        "Căn cứ: NĐ 30/2020/NĐ-CP và Danh mục HS năm/Kế hoạch thu thập tài liệu.\n\n" +
                        "BÊN GIAO: Tên cá nhân/đơn vị giao nộp — họ tên, chức vụ.\n" +
                        "BÊN NHẬN: Lưu trữ cơ quan — họ tên, chức vụ.\n\n" +
                        "Nội dung giao nhận:\n" +
                        "1. Tên khối tài liệu giao nộp\n2. Thời gian của HS, tài liệu\n" +
                        "3. Số lượng: Hộp/cặp, HS (quy ra mét giá) — Giấy; Tổng HS, tổng tệp tin — Điện tử\n" +
                        "4. Tình trạng tài liệu\n5. Mục lục HS kèm theo\n\n" +
                        "Biên bản lập thành 2 bản: bên giao 1, bên nhận 1." },
            });

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
            phuLuc6.Children.AddRange(new[]
            {
                new LegalNode { Id = "pl6-nguyen-tac", Title = "I–II. Nguyên tắc & Yêu cầu chung", NodeType = LegalNodeType.SubSection,
                    Content = "I. NGUYÊN TẮC XÂY DỰNG HỆ THỐNG (5 nguyên tắc):\n" +
                        "1. Quản lý VB và HS điện tử đúng quy định.\n" +
                        "2. An toàn, an ninh thông tin mạng theo pháp luật.\n" +
                        "3. Phân quyền truy cập cho cá nhân.\n" +
                        "4. Tính xác thực, độ tin cậy của tài liệu, dữ liệu.\n" +
                        "5. Cho phép kiểm chứng, xác minh khi được yêu cầu.\n\n" +
                        "II. YÊU CẦU CHUNG KHI THIẾT KẾ (8 yêu cầu):\n" +
                        "1. Đáp ứng đầy đủ quy trình quản lý VB điện tử, lập hồ sơ, dữ liệu đặc tả.\n" +
                        "2. Tích hợp, liên thông, chia sẻ với các hệ thống khác.\n" +
                        "3. Hệ thống hóa VB, HS, thống kê lượt truy cập.\n" +
                        "4. Xác thực, tin cậy, toàn vẹn, khả năng truy cập.\n" +
                        "5. Lưu trữ HS theo thời hạn bảo quản.\n" +
                        "6. Phù hợp Khung kiến trúc Chính phủ điện tử VN.\n" +
                        "7. Dễ tiếp cận và sử dụng.\n" +
                        "8. Ký số, kiểm tra, xác thực chữ ký số theo pháp luật." },
                new LegalNode { Id = "pl6-chuc-nang", Title = "III. Yêu cầu chức năng (8 nhóm)", NodeType = LegalNodeType.SubSection,
                    Content = "1. TẠO LẬP VÀ THEO DÕI VB: Tạo mới, đính kèm, mã định danh, cấp số tự động, thông báo VB mới, theo dõi đôn đốc.\n\n" +
                        "2. KẾT NỐI, LIÊN THÔNG: Liên thông giữa các HTQLTL, hoạt động trên thiết bị di động, tích hợp hệ thống chuyên dụng.\n\n" +
                        "3. AN NINH THÔNG TIN: Các cấp độ an ninh, phân quyền truy cập từng HS/VB, cảnh báo thay đổi quyền.\n\n" +
                        "4. LẬP VÀ QUẢN LÝ HỒ SƠ: Tạo danh mục HS, mã HS, đánh số VB tự động, liên kết VB cùng mã HS, gán VB cho nhiều HS.\n\n" +
                        "5. BẢO QUẢN VÀ LƯU TRỮ: Lưu VB + quá trình giải quyết, thông báo HS đến hạn nộp lưu, sao lưu định kỳ, phục hồi dữ liệu.\n\n" +
                        "6. THỐNG KÊ, TÌM KIẾM: Thống kê số lượng HS/VB/lượt truy cập, tìm kiếm tất cả trường + nội dung, kết xuất doc/pdf.\n\n" +
                        "7. QUẢN LÝ DỮ LIỆU ĐẶC TẢ: Lưu, hiển thị, bổ sung dữ liệu đặc tả VB/HS, cố định liên kết.\n\n" +
                        "8. THU HỒI VB: Đóng băng VB đi khi thu hồi, hủy VB đến khi có lệnh, lưu dữ liệu đặc tả quá trình thu hồi." },
                new LegalNode { Id = "pl6-quan-tri", Title = "IV–V. Quản trị & Thông tin đầu ra", NodeType = LegalNodeType.SubSection,
                    Content = "IV. YÊU CẦU QUẢN TRỊ HỆ THỐNG:\n" +
                        "1. Người quản trị được phép:\n" +
                        "   a) Tạo nhóm tài liệu/HS theo cấp độ thông tin\n" +
                        "   b) Phân quyền người sử dụng\n" +
                        "   c) Truy cập HS và dữ liệu đặc tả\n" +
                        "   d) Thay đổi quyền truy cập khi quy định thay đổi\n" +
                        "   đ) Thay đổi quyền khi thay đổi vị trí công tác\n" +
                        "   e) Phục hồi thông tin khi lỗi hệ thống\n" +
                        "   g) Khóa/đóng băng tập hợp VB, HS khi có yêu cầu\n" +
                        "2. Cảnh báo xung đột trong hệ thống.\n" +
                        "3. Thiết lập kết nối liên thông.\n\n" +
                        "V. THÔNG TIN ĐẦU RA (6 loại):\n" +
                        "1. Sổ đăng ký VB đến\n2. Báo cáo tình hình giải quyết VB đến\n" +
                        "3. Sổ đăng ký VB đi\n4. Báo cáo tình hình giải quyết VB đi\n" +
                        "5. Mục lục VB trong HS\n6. Mục lục hồ sơ" },
                new LegalNode { Id = "pl6-chuan-thong-tin", Title = "Phần II. Chuẩn thông tin đầu vào", NodeType = LegalNodeType.SubSection,
                    Content = "I. THÔNG TIN ĐẦU VÀO — VĂN BẢN ĐI (16 trường, 24 dòng dữ liệu):\n\n" +
                        "| STT | Trường thông tin | Tên viết tắt tiếng Anh | Kiểu dữ liệu | Độ dài |\n" +
                        "|-----|-----------------|----------------------|--------------|--------|\n" +
                        "| 1 | Mã hồ sơ | FileCode | | |\n" +
                        "| 1.1 | Mã định danh CQ | OrganId | String | 13 |\n" +
                        "| 1.2 | Năm hình thành HS | FileCatalog | Number | 4 |\n" +
                        "| 1.3 | Số và ký hiệu HS | FileNotation | String | 20 |\n" +
                        "| 2 | STT VB trong HS | DocOrdinal | Number | 3 |\n" +
                        "| 3 | Tên loại văn bản | TypeName | String | 100 |\n" +
                        "| 4 | Số của văn bản | CodeNumber | String | 11 |\n" +
                        "| 5 | Ký hiệu văn bản | CodeNotation | String | 30 |\n" +
                        "| 6 | Ngày, tháng, năm VB | IssuedDate | Date | 10 |\n" +
                        "| 7 | Tên CQ ban hành | OrganName | String | 200 |\n" +
                        "| 8 | Trích yếu nội dung | Subject | String | 500 |\n" +
                        "| 9 | Ngôn ngữ | Language | String | 30 |\n" +
                        "| 10 | Số trang văn bản | PageAmount | Number | 3 |\n" +
                        "| 11 | Ghi chú | Description | String | 500 |\n" +
                        "| 12 | Chức vụ, họ tên người ký | SignerInfo | | |\n" +
                        "| 12.1 | Chức vụ | Position | String | 100 |\n" +
                        "| 12.2 | Họ tên | FullName | String | 50 |\n" +
                        "| 13 | Nơi nhận | To | | |\n" +
                        "| 13.1 | Mã định danh CQ nhận | OrganId | String | 13 |\n" +
                        "| 13.2 | Tên CQ nhận | OrganName | String | 200 |\n" +
                        "| 14 | Mức độ khẩn | Priority | Number | 1 |\n" +
                        "| 15 | Số lượng bản phát hành | IssuedAmount | Number | 3 |\n" +
                        "| 16 | Hạn trả lời văn bản | DueDate | Date | 10 |\n\n" +
                        "II. THÔNG TIN ĐẦU VÀO — VĂN BẢN ĐẾN (18 trường):\n" +
                        "Gồm các trường của VB đi (1–16), bổ sung thêm:\n\n" +
                        "| STT | Trường thông tin | Tên viết tắt tiếng Anh | Kiểu dữ liệu | Độ dài |\n" +
                        "|-----|-----------------|----------------------|--------------|--------|\n" +
                        "| 12 | Ngày, tháng, năm đến | ArrivalDate | Date | 10 |\n" +
                        "| 13 | Số đến | ArrivalNumber | Number | 10 |\n" +
                        "| 16 | Đơn vị/người nhận | ToPlaces | String | 1000 |\n" +
                        "| 17 | Ý kiến phân phối, chỉ đạo | TraceHeaderList | LongText | |\n" +
                        "| 18 | Thời hạn giải quyết | DueDate | Date | 10 |\n\n" +
                        "III. THÔNG TIN ĐẦU VÀO — HỒ SƠ (11 trường, 15 dòng dữ liệu):\n\n" +
                        "| STT | Trường thông tin | Tên viết tắt tiếng Anh | Kiểu dữ liệu | Độ dài |\n" +
                        "|-----|-----------------|----------------------|--------------|--------|\n" +
                        "| 1 | Mã hồ sơ | FileCode | | |\n" +
                        "| 1.1 | Mã định danh CQ | OrganId | String | 13 |\n" +
                        "| 1.2 | Năm hình thành HS | FileCatalog | Number | 4 |\n" +
                        "| 1.3 | Số và ký hiệu HS | FileNotation | String | 20 |\n" +
                        "| 2 | Tiêu đề hồ sơ | Title | String | 500 |\n" +
                        "| 3 | Thời hạn bảo quản | Maintenance | String | 30 |\n" +
                        "| 4 | Chế độ sử dụng | Rights | String | 30 |\n" +
                        "| 5 | Người lập hồ sơ | Creator | String | 30 |\n" +
                        "| 6 | Ngôn ngữ | Language | String | 50 |\n" +
                        "| 7 | Thời gian bắt đầu | StartDate | Date | 10 |\n" +
                        "| 8 | Thời gian kết thúc | EndDate | Date | 10 |\n" +
                        "| 9 | Tổng số VB trong HS | DocTotal | Number | 4 |\n" +
                        "| 10 | Tổng số trang của HS | PageTotal | Number | 4 |\n" +
                        "| 11 | Ghi chú | Description | String | 500 |" },
            });

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
