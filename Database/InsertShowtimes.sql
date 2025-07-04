-- Script to insert Showtime data
-- Suất chiếu cho các phim đang chiếu (phim có Id từ 1-5) tại các rạp có Id từ 1-8
-- Thêm suất chiếu cho hôm nay và ngày mai

-- Sử dụng BEGIN...END để giữ phạm vi của biến
BEGIN
    -- Lấy ngày hiện tại và ngày mai (sử dụng DATETIME thay vì DATE)
    DECLARE @Today DATETIME = CAST(GETDATE() AS DATE) -- Chuyển về đầu ngày (00:00:00)
    DECLARE @Tomorrow DATETIME = DATEADD(DAY, 1, @Today)

    -- Suất chiếu cho phim "Vũ Trụ Anh Hùng" (Id=1) tại các rạp 
    -- Suất chiếu hôm nay
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Hà Nội (Id=1)
    (1, 1, DATEADD(HOUR, 10, @Today), N'2D', 80000, 200),
    (1, 1, DATEADD(HOUR, 13, @Today), N'3D', 100000, 200),
    (1, 1, DATEADD(HOUR, 16, @Today), N'2D', 90000, 200),
    (1, 1, DATEADD(HOUR, 19, @Today), N'IMAX', 120000, 200),
    -- Tại MovieStar Sài Gòn (Id=2)
    (1, 2, DATEADD(HOUR, 9, @Today), N'2D', 85000, 250),
    (1, 2, DATEADD(HOUR, 12, @Today), N'3D', 105000, 250),
    (1, 2, DATEADD(HOUR, 15, @Today), N'2D', 95000, 250),
    (1, 2, DATEADD(HOUR, 18, @Today), N'IMAX', 125000, 250);

    -- Suất chiếu ngày mai cho phim "Vũ Trụ Anh Hùng" (Id=1) 
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Hà Nội (Id=1)
    (1, 1, DATEADD(HOUR, 10, @Tomorrow), N'2D', 80000, 200),
    (1, 1, DATEADD(HOUR, 13, @Tomorrow), N'3D', 100000, 200),
    (1, 1, DATEADD(HOUR, 16, @Tomorrow), N'2D', 90000, 200),
    (1, 1, DATEADD(HOUR, 19, @Tomorrow), N'IMAX', 120000, 200),
    -- Tại MovieStar Sài Gòn (Id=2)
    (1, 2, DATEADD(HOUR, 9, @Tomorrow), N'2D', 85000, 250),
    (1, 2, DATEADD(HOUR, 12, @Tomorrow), N'3D', 105000, 250),
    (1, 2, DATEADD(HOUR, 15, @Tomorrow), N'2D', 95000, 250),
    (1, 2, DATEADD(HOUR, 18, @Tomorrow), N'IMAX', 125000, 250);

    -- Suất chiếu cho phim "Tình Yêu Và Số Phận" (Id=2) tại các rạp
    -- Suất chiếu hôm nay
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Đà Nẵng (Id=3)
    (2, 3, DATEADD(HOUR, 10, @Today), N'2D', 75000, 180),
    (2, 3, DATEADD(HOUR, 14, @Today), N'2D', 85000, 180),
    (2, 3, DATEADD(HOUR, 18, @Today), N'2D', 95000, 180),
    -- Tại MovieStar Cần Thơ (Id=4)
    (2, 4, DATEADD(HOUR, 11, @Today), N'2D', 70000, 150),
    (2, 4, DATEADD(HOUR, 15, @Today), N'2D', 80000, 150),
    (2, 4, DATEADD(HOUR, 19, @Today), N'2D', 90000, 150);

    -- Suất chiếu ngày mai cho phim "Tình Yêu Và Số Phận" (Id=2)
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Đà Nẵng (Id=3)
    (2, 3, DATEADD(HOUR, 10, @Tomorrow), N'2D', 75000, 180),
    (2, 3, DATEADD(HOUR, 14, @Tomorrow), N'2D', 85000, 180),
    (2, 3, DATEADD(HOUR, 18, @Tomorrow), N'2D', 95000, 180),
    -- Tại MovieStar Cần Thơ (Id=4)
    (2, 4, DATEADD(HOUR, 11, @Tomorrow), N'2D', 70000, 150),
    (2, 4, DATEADD(HOUR, 15, @Tomorrow), N'2D', 80000, 150),
    (2, 4, DATEADD(HOUR, 19, @Tomorrow), N'2D', 90000, 150);

    -- Suất chiếu cho phim "Quỷ Quyệt: Cánh Cửa Đỏ" (Id=3) tại các rạp
    -- Suất chiếu hôm nay
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Huế (Id=5)
    (3, 5, DATEADD(HOUR, 10, @Today), N'2D', 65000, 120),
    (3, 5, DATEADD(HOUR, 15, @Today), N'2D', 75000, 120),
    (3, 5, DATEADD(HOUR, 20, @Today), N'2D', 85000, 120),
    -- Tại MovieStar Nha Trang (Id=6)
    (3, 6, DATEADD(HOUR, 11, @Today), N'2D', 70000, 160),
    (3, 6, DATEADD(HOUR, 16, @Today), N'2D', 80000, 160),
    (3, 6, DATEADD(HOUR, 21, @Today), N'2D', 90000, 160);

    -- Suất chiếu ngày mai cho phim "Quỷ Quyệt: Cánh Cửa Đỏ" (Id=3)
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Huế (Id=5)
    (3, 5, DATEADD(HOUR, 10, @Tomorrow), N'2D', 65000, 120),
    (3, 5, DATEADD(HOUR, 15, @Tomorrow), N'2D', 75000, 120),
    (3, 5, DATEADD(HOUR, 20, @Tomorrow), N'2D', 85000, 120),
    -- Tại MovieStar Nha Trang (Id=6)
    (3, 6, DATEADD(HOUR, 11, @Tomorrow), N'2D', 70000, 160),
    (3, 6, DATEADD(HOUR, 16, @Tomorrow), N'2D', 80000, 160),
    (3, 6, DATEADD(HOUR, 21, @Tomorrow), N'2D', 90000, 160);

    -- Suất chiếu cho phim "Cuộc Phiêu Lưu Của Mèo Tom" (Id=4) tại các rạp
    -- Suất chiếu hôm nay
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Hải Phòng (Id=7)
    (4, 7, DATEADD(HOUR, 9, @Today), N'2D', 60000, 140),
    (4, 7, DATEADD(HOUR, 13, @Today), N'3D', 80000, 140),
    (4, 7, DATEADD(HOUR, 17, @Today), N'2D', 70000, 140),
    -- Tại MovieStar Vũng Tàu (Id=8)
    (4, 8, DATEADD(HOUR, 10, @Today), N'2D', 65000, 130),
    (4, 8, DATEADD(HOUR, 14, @Today), N'3D', 85000, 130),
    (4, 8, DATEADD(HOUR, 18, @Today), N'2D', 75000, 130);

    -- Suất chiếu ngày mai cho phim "Cuộc Phiêu Lưu Của Mèo Tom" (Id=4)
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Hải Phòng (Id=7)
    (4, 7, DATEADD(HOUR, 9, @Tomorrow), N'2D', 60000, 140),
    (4, 7, DATEADD(HOUR, 13, @Tomorrow), N'3D', 80000, 140),
    (4, 7, DATEADD(HOUR, 17, @Tomorrow), N'2D', 70000, 140),
    -- Tại MovieStar Vũng Tàu (Id=8)
    (4, 8, DATEADD(HOUR, 10, @Tomorrow), N'2D', 65000, 130),
    (4, 8, DATEADD(HOUR, 14, @Tomorrow), N'3D', 85000, 130),
    (4, 8, DATEADD(HOUR, 18, @Tomorrow), N'2D', 75000, 130);

    -- Suất chiếu cho phim "Những Người Lính Cuối Cùng" (Id=5) tại các rạp
    -- Suất chiếu hôm nay
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Hà Nội (Id=1)
    (5, 1, DATEADD(HOUR, 11, @Today), N'2D', 80000, 200),
    (5, 1, DATEADD(HOUR, 15, @Today), N'2D', 90000, 200),
    (5, 1, DATEADD(HOUR, 19, @Today), N'2D', 100000, 200),
    -- Tại MovieStar Sài Gòn (Id=2)
    (5, 2, DATEADD(HOUR, 10, @Today), N'2D', 85000, 250),
    (5, 2, DATEADD(HOUR, 14, @Today), N'2D', 95000, 250),
    (5, 2, DATEADD(HOUR, 18, @Today), N'2D', 105000, 250);

    -- Suất chiếu ngày mai cho phim "Những Người Lính Cuối Cùng" (Id=5)
    INSERT INTO Showtimes (MovieId, TheaterId, ShowDateTime, ScreenType, Price, AvailableSeats)
    VALUES 
    -- Tại MovieStar Hà Nội (Id=1)
    (5, 1, DATEADD(HOUR, 11, @Tomorrow), N'2D', 80000, 200),
    (5, 1, DATEADD(HOUR, 15, @Tomorrow), N'2D', 90000, 200),
    (5, 1, DATEADD(HOUR, 19, @Tomorrow), N'2D', 100000, 200),
    -- Tại MovieStar Sài Gòn (Id=2)
    (5, 2, DATEADD(HOUR, 10, @Tomorrow), N'2D', 85000, 250),
    (5, 2, DATEADD(HOUR, 14, @Tomorrow), N'2D', 95000, 250),
    (5, 2, DATEADD(HOUR, 18, @Tomorrow), N'2D', 105000, 250);
END 