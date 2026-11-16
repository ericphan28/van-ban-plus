namespace AIVanBan.Core.Services;

/// <summary>
/// Nhãn hiển thị (có biểu tượng) cho các ô nhập của hộp thoại soạn thảo. Chỉ là chữ giao diện, KHÔNG phải prompt:
/// prompt nằm ở máy chủ (vanbanplus-api, tác vụ soan-thao).
/// </summary>
public static class NhanTruongSoanThao
{
    public static string GetFieldLabel(string field)
    {
        return field switch
        {
            // === Thông tin cơ quan ===
            "from_org" => "🏢 Cơ quan ban hành",
            "to_org" => "📨 Cơ quan nhận",
            "to_department" => "🏛️ Sở/Ban/Ngành nhận",
            "org_name" => "🏛️ Tên cơ quan/tổ chức",
            "from_unit" => "🏢 Đơn vị cũ",
            "to_unit" => "🏢 Đơn vị mới/nhận",
            "copy_org" => "🏢 Cơ quan sao lục",
            "implementing_unit" => "⚙️ Đơn vị thực hiện",
            
            // === Nội dung văn bản ===
            "subject" => "📋 Vấn đề/Tiêu đề",
            "content" => "📝 Nội dung chính",
            "reason" => "💡 Lý do",
            "purpose" => "🎯 Mục đích",
            "proposal" => "📊 Đề xuất",
            "proposals" => "💡 Đề xuất, kiến nghị",
            "objectives" => "🎯 Mục tiêu",
            "tasks" => "📋 Nhiệm vụ",
            "articles" => "📜 Các điều khoản",
            "program" => "📜 Chương trình",
            "agenda" => "📋 Nội dung cuộc họp",
            "conclusion" => "✅ Kết luận",
            "legal_basis" => "📖 Căn cứ pháp lý",
            
            // === Người ký / Người liên quan ===
            "signer_name" => "✍️ Người ký",
            "signer_title" => "👔 Chức danh người ký",
            "signer" => "✍️ Người ký",
            "chairman_name" => "👨‍💼 Chủ tịch",
            "principal_name" => "👨‍💼 Hiệu trưởng",
            "person_name" => "👤 Họ tên cán bộ",
            "person" => "👤 Họ tên",
            "citizen_name" => "👤 Họ tên công dân",
            "patient_name" => "🧑‍⚕️ Họ tên bệnh nhân",
            "student_name" => "🎓 Họ tên học sinh",
            "recipient" => "📬 Đơn vị/Người nhận",
            "recipients" => "📬 Nơi nhận",
            "grantor" => "👤 Người ủy quyền",
            "grantee" => "👤 Người được ủy quyền",
            "participants" => "👥 Thành phần tham dự",
            "members" => "👥 Danh sách thành viên",
            "students" => "🎓 Danh sách học sinh",
            "beneficiaries" => "👥 Đối tượng thụ hưởng",
            
            // === Chức vụ / Vị trí ===
            "current_position" => "💼 Chức vụ hiện tại",
            "new_position" => "⭐ Chức vụ mới",
            "level" => "🏛️ Cấp (Tỉnh/Xã)",
            "ranking" => "🏅 Xếp loại",
            
            // === Thời gian / Địa điểm ===
            "time" => "⏰ Thời gian",
            "time_place" => "⏰ Thời gian, địa điểm",
            "location" => "📍 Địa điểm",
            "address" => "📍 Địa chỉ",
            "effective_date" => "📅 Ngày hiệu lực",
            "period" => "📆 Kỳ báo cáo/kế hoạch",
            "from_date" => "📅 Từ ngày",
            "to_date" => "📅 Đến ngày",
            "birth_date" => "📅 Ngày sinh",
            "exam_date" => "📅 Ngày khám",
            "meeting_time" => "⏰ Thời gian họp",
            "graduation_year" => "📅 Năm tốt nghiệp",
            "school_year" => "📅 Năm học",
            "year" => "📅 Năm",
            
            // === Khen thưởng / Kỷ luật ===
            "award_type" => "🏆 Hình thức khen thưởng",
            "achievement" => "✨ Thành tích",
            "achievements" => "✅ Kết quả đạt được",
            "reward_type" => "🏆 Hình thức khen thưởng",
            "reward_proposal" => "🏆 Đề nghị khen thưởng",
            "collective_achievements" => "✅ Thành tích tập thể",
            "violation" => "⚠️ Hành vi vi phạm",
            "penalty" => "⚖️ Hình thức xử phạt",
            "discipline_type" => "⚠️ Hình thức kỷ luật",
            
            // === Báo cáo / Đánh giá ===
            "situation" => "📊 Tình hình",
            "results" => "📈 Kết quả",
            "result" => "📈 Kết quả",
            "challenges" => "⚠️ Tồn tại, hạn chế",
            "future_plans" => "🚀 Phương hướng tiếp theo",
            "next_plan" => "🚀 Kế hoạch tiếp theo",
            "evaluation" => "⭐ Đánh giá",
            "task_name" => "📌 Nhiệm vụ/Kế hoạch",
            "field" => "📂 Lĩnh vực",
            "solutions" => "💡 Giải pháp thực hiện",
            "targets" => "🎯 Chỉ tiêu",
            "implementation" => "⚙️ Tổ chức thực hiện",
            "year_targets" => "🎯 Chỉ tiêu năm",
            "criteria_status" => "📊 Tình trạng các tiêu chí",
            
            // === Sự kiện / Hội nghị ===
            "event_name" => "🎉 Tên sự kiện",
            "meeting_name" => "🤝 Tên cuộc họp",
            "reply_to_number" => "🔢 Trả lời công văn số",
            
            // === Dự án / Tài chính ===
            "project_name" => "🎯 Tên đề án/dự án",
            "budget" => "💰 Kinh phí",
            "support_type" => "📋 Hình thức hỗ trợ",
            "support_amount" => "💰 Mức hỗ trợ",
            
            // === Trường học ===
            "school_name" => "🏫 Tên trường",
            "grade" => "📚 Khối/Lớp",
            "class_name" => "📚 Tên lớp",
            "curriculum_plan" => "📖 Chương trình dạy học",
            "student_count" => "👥 Số lượng học sinh",
            "quality_stats" => "📊 Thống kê chất lượng",
            
            // === Y tế ===
            "medical_unit" => "🏥 Cơ sở y tế",
            "hospital" => "🏥 Bệnh viện",
            "from_hospital" => "🏥 Bệnh viện chuyển",
            "to_hospital" => "🏥 Bệnh viện nhận",
            "disease_name" => "🩺 Tên bệnh/dịch",
            "diagnosis" => "🩺 Chẩn đoán",
            "transfer_reason" => "📋 Lý do chuyển viện",
            "prevention_measures" => "🛡️ Biện pháp phòng chống",
            "measures" => "📋 Biện pháp thực hiện",
            "statistics" => "📊 Số liệu thống kê",
            "patient_count" => "👥 Số lượng bệnh nhân",
            "clinical_results" => "📈 Kết quả lâm sàng",
            "treatment_plan" => "📋 Phác đồ điều trị",
            "procedure_name" => "📋 Tên quy trình",
            "test_type" => "🔬 Loại xét nghiệm",
            "test_result" => "📈 Kết quả xét nghiệm",
            "area" => "📍 Khu vực/Địa bàn",
            
            // === Hành chính xã/phường ===
            "ward_name" => "🏘️ Tên xã/phường",
            "marital_status" => "👪 Tình trạng hôn nhân",
            "population" => "👥 Dân số",
            "birth_death_rate" => "📊 Tỷ lệ sinh/tử",
            "economy" => "📈 Kinh tế",
            "social" => "🏘️ Xã hội",
            "disaster_type" => "⚠️ Loại thiên tai",
            "risk_areas" => "📍 Vùng có nguy cơ",
            "rescue_forces" => "🚑 Lực lượng cứu hộ",
            "evacuation_plan" => "🗺️ Phương án sơ tán",
            "reform_content" => "📋 Nội dung cải cách",
            "procedures" => "📋 Thủ tục",
            
            // === Sao lục / Phụ lục ===
            "original_document" => "📄 Văn bản gốc",
            "original_saoy" => "📄 Bản gốc sao y",
            "extract_section" => "📋 Phần trích sao",
            "document_ref" => "📎 Số hiệu văn bản",
            "documents" => "📎 Danh sách văn bản",
            "parent_document" => "📄 Văn bản chính",
            "appendix_title" => "📋 Tiêu đề phụ lục",
            "appendix_number" => "🔢 Số thứ tự phụ lục",
            
            // === Hợp đồng / Biên bản ===
            "party_a" => "🤝 Bên A",
            "party_b" => "🤝 Bên B",
            "attendees" => "👥 Thành phần tham dự",
            "timeline" => "⏰ Thời gian thực hiện",
            "instructions" => "📝 Ý kiến chỉ đạo",
            "letter_type" => "📋 Loại thư",
            "duration" => "⏱️ Thời hạn",
            
            // === Y tế bổ sung ===
            "patient_age" => "📅 Tuổi bệnh nhân",
            "patient_gender" => "⚧ Giới tính bệnh nhân",
            "medical_summary" => "📋 Tóm tắt bệnh án",
            "birth_year" => "📅 Năm sinh",
            "scope" => "📋 Phạm vi áp dụng",
            "discussion" => "💬 Ý kiến thảo luận",
            "recommendation" => "💡 Khuyến nghị",
            "recommendations" => "💡 Kiến nghị, đề xuất",
            
            // === Giáo dục bổ sung ===
            "assessment" => "📝 Kiểm tra đánh giá",
            "improvement_plan" => "📋 Kế hoạch cải thiện",
            "class_info" => "📚 Khối/Lớp liên quan",
            "notes" => "📝 Ghi chú, lưu ý",
            "teacher_achievements" => "👩‍🏫 Thành tích giáo viên",
            "student_achievements" => "🎓 Thành tích học sinh",
            
            // === UBND xã bổ sung ===
            "id_number" => "🆔 Số CCCD/CMND",
            "resources" => "📦 Phương tiện, vật tư",
            "funding_source" => "💰 Nguồn kinh phí",
            "funding" => "💰 Nguồn vốn",
            "assignment" => "👥 Phân công thực hiện",
            "receiving_point" => "📍 Nơi tiếp nhận",
            "contact" => "📞 Liên hệ",
            "family_planning_results" => "📊 Kết quả KHHGĐ",
            
            _ => $"📝 {FormatFieldName(field)}"
        };
    }
    
    /// <summary>
    /// Chuyển field name kỹ thuật thành tên thân thiện (fallback)
    /// VD: "school_name" → "Tên trường", "from_date" → "Từ ngày"
    /// </summary>
    public static string FormatFieldName(string field)
    {
        // Thay _ thành khoảng trắng, viết hoa chữ đầu
        var words = field.Split('_');
        return string.Join(" ", words.Select(w => 
            w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
    }
}
