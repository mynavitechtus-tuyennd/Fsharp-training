// ==========================================
// PHẦN 1: MÔ HÌNH HÓA
// ==========================================
type Grade =
    | A
    | B
    | C
    | D
    | F


type Student = {
    Id: int
    Name: string
    Score: int
}

// ==========================================
// PHẦN 2: LUYỆN TẬP PATTERN MATCHING NÂNG CAO
// ==========================================

// BÀI TẬP 1: Xếp loại học lực (Sử dụng Match với điều kiện 'when')
// Yêu cầu:
// - Score >= 90 -> A
// - Score >= 80 -> B
// - Score >= 70 -> C
// - Score >= 50 -> D
// - Còn lại (< 50) -> F

let calculateGrade (score: int) =
    match score with
        | score when score >= 90 -> A
        | score when score >= 80 -> B
        | score when score >= 70 -> C
        | score when score >= 50 -> D
        | _ -> F

// ==========================================
// PHẦN 3: XỬ LÝ DANH SÁCH (List Module)
// ==========================================

// BÀI TẬP 2: Lọc học sinh giỏi (Dùng List.filter)
// Yêu cầu: Lấy ra các học sinh có điểm >= 80
let getExcellentStudents (students: Student list) =
    students
    |> List.filter (fun student -> student.Score >= 80)

// BÀI TẬP 3: Chỉ lấy tên học sinh (Dùng List.map)
// Yêu cầu: Từ danh sách Student, biến đổi thành danh sách string (chỉ chứa Name)
let getStudentNames (students: Student list) =
    students
    |> List.map (fun student -> student.Name)

// BÀI TẬP 4: Tính điểm trung bình (Dùng List.averageBy)
// Yêu cầu: Tính trung bình cộng cột Score của cả lớp. 
// Lưu ý: List.averageBy yêu cầu kết quả là số thực (float), nên bạn cần ép kiểu (float student.Score)
let getClassAverage (students: Student list) =
    List.averageBy(fun student -> float student.Score) students

// ==========================================
// PHẦN 4: MASTER PIPELINE (Kết hợp tất cả)
// ==========================================

// BÀI TẬP 5: Tìm top 3 học sinh xuất sắc nhất
// Yêu cầu: Hãy nối các thao tác bằng toán tử đường ống (|>)
// 1. Nhận vào danh sách students
// 2. Lọc ra những người đậu (Score >= 50) bằng List.filter
// 3. Sắp xếp điểm giảm dần bằng List.sortByDescending (fun s -> s.Score)
// 4. Lấy tối đa 3 người đứng đầu bằng List.truncate 3 (Dùng truncate an toàn hơn take vì lỡ danh sách < 3 người thì không bị lỗi)
// 5. Biến đổi danh sách đó thành danh sách Tên (Dùng hàm getStudentNames bạn đã viết ở Bài 3)

let getTop3PassingNames (students: Student list) =
    students
    |> List.filter (fun student -> student.Score >= 50)
    |> List.sortByDescending(fun s -> s.Score)
    |> List.truncate 3
    |> getStudentNames 


