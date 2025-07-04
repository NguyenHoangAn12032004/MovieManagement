-- Script to insert movie data with Vietnamese content
-- 5 Now Showing Movies (IsNowShowing = 1)
INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(1, N'Vũ Trụ Anh Hùng', 
   N'Câu chuyện về một nhóm siêu anh hùng phải cứu thế giới khỏi thế lực hắc ám đang đe dọa sự sống còn của nhân loại.', 
   '2023-06-15', 145, 
   N'/images/movies/movie1.jpg', 
   N'/images/backdrops/backdrop1.jpg', 
   8.5, N'Active', 
   N'[1, 9, 15]', N'["Hành Động", "Giả Tưởng", "Khoa Học Viễn Tưởng"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=abc123', 
   N'Nguyễn Văn A, Trần Thị B, Lê Văn C', 
   N'Phạm Đức D', 
   1, 0, 1,
   GETUTCDATE(), GETUTCDATE());

INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(2, N'Tình Yêu Và Số Phận', 
   N'Câu chuyện tình yêu đầy cảm động giữa hai người xa lạ gặp nhau trong hoàn cảnh khó khăn và phải vượt qua nhiều thử thách.', 
   '2023-05-20', 125, 
   N'/images/movies/movie2.jpg', 
   N'/images/backdrops/backdrop2.jpg', 
   7.8, N'Active', 
   N'[7, 14]', N'["Chính Kịch", "Lãng Mạn"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=def456', 
   N'Lý Thị E, Hoàng Văn F, Ngô Thị G', 
   N'Vũ Minh H', 
   1, 0, 1,
   GETUTCDATE(), GETUTCDATE());

INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(3, N'Quỷ Quyệt: Cánh Cửa Đỏ', 
   N'Một gia đình chuyển đến ngôi nhà mới và phát hiện ra những hiện tượng kỳ lạ. Họ phải đối mặt với thế lực siêu nhiên đang đe dọa cuộc sống của họ.', 
   '2023-06-01', 110, 
   N'/images/movies/movie3.jpg', 
   N'/images/backdrops/backdrop3.jpg', 
   7.2, N'Active', 
   N'[11, 13]', N'["Kinh Dị", "Bí Ẩn"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=ghi789', 
   N'Đỗ Văn I, Bùi Thị J, Trương Văn K', 
   N'Mai Anh L', 
   1, 0, 0,
   GETUTCDATE(), GETUTCDATE());

INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(4, N'Cuộc Phiêu Lưu Của Mèo Tom', 
   N'Câu chuyện vui nhộn về chú mèo Tom và những người bạn trong cuộc phiêu lưu đầy bất ngờ để giải cứu khu rừng khỏi những kẻ phá hoại.', 
   '2023-04-10', 95, 
   N'/images/movies/movie4.jpg', 
   N'/images/backdrops/backdrop4.jpg', 
   8.0, N'Active', 
   N'[3, 4, 8]', N'["Hoạt Hình", "Hài", "Gia Đình"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=jkl012', 
   N'Hà Văn M, Phan Thị N, Dương Văn O', 
   N'Lâm Thành P', 
   1, 0, 0,
   GETUTCDATE(), GETUTCDATE());

INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(5, N'Những Người Lính Cuối Cùng', 
   N'Câu chuyện cảm động về những người lính trong cuộc chiến tranh, họ phải đối mặt với cái chết và tình bạn để bảo vệ đất nước.', 
   '2023-05-05', 140, 
   N'/images/movies/movie5.jpg', 
   N'/images/backdrops/backdrop5.jpg', 
   8.3, N'Active', 
   N'[7, 10, 18]', N'["Chính Kịch", "Lịch Sử", "Chiến Tranh"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=mno345', 
   N'Trịnh Văn Q, Cao Thị R, Đinh Văn S', 
   N'Nguyễn Bá T', 
   1, 0, 0,
   GETUTCDATE(), GETUTCDATE());

-- 5 Coming Soon Movies (IsComingSoon = 1)
INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(6, N'Vũ Điệu Đam Mê', 
   N'Câu chuyện về một vũ công trẻ tài năng phải vượt qua những thử thách và định kiến để theo đuổi ước mơ của mình.', 
   '2024-01-15', 118, 
   N'/images/movies/movie6.jpg', 
   N'/images/backdrops/backdrop6.jpg', 
   0, N'Coming Soon', 
   N'[7, 12]', N'["Chính Kịch", "Âm Nhạc"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=pqr678', 
   N'Lê Thị U, Phạm Văn V, Hoàng Thị X', 
   N'Trần Minh Y', 
   0, 1, 1,
   GETUTCDATE(), GETUTCDATE());

INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(7, N'Bóng Ma Trong Nhà', 
   N'Một gia đình trẻ chuyển đến ngôi nhà cổ và phát hiện ra những hiện tượng kỳ bí. Họ phải khám phá bí mật kinh hoàng của ngôi nhà.', 
   '2023-12-25', 105, 
   N'/images/movies/movie7.jpg', 
   N'/images/backdrops/backdrop7.jpg', 
   0, N'Coming Soon', 
   N'[11, 13]', N'["Kinh Dị", "Bí Ẩn"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=stu901', 
   N'Ngô Văn Z, Lý Thị AA, Vũ Văn BB', 
   N'Đặng Hồng CC', 
   0, 1, 1,
   GETUTCDATE(), GETUTCDATE());

INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(8, N'Siêu Trộm: Phi Vụ Cuối Cùng', 
   N'Câu chuyện hồi hộp về một nhóm trộm chuyên nghiệp thực hiện phi vụ cuối cùng, đầy rủi ro và nguy hiểm.', 
   '2024-02-10', 130, 
   N'/images/movies/movie8.jpg', 
   N'/images/backdrops/backdrop8.jpg', 
   0, N'Coming Soon', 
   N'[1, 5, 13]', N'["Hành Động", "Tội Phạm", "Bí Ẩn"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=vwx234', 
   N'Đinh Văn DD, Mai Thị EE, Trương Văn FF', 
   N'Phan Quốc GG', 
   0, 1, 0,
   GETUTCDATE(), GETUTCDATE());

INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(9, N'Vùng Đất Diệu Kỳ', 
   N'Cuộc phiêu lưu của một nhóm bạn trẻ đến một vùng đất bí ẩn, nơi họ khám phá ra những sinh vật kỳ lạ và phép thuật.', 
   '2024-01-05', 115, 
   N'/images/movies/movie9.jpg', 
   N'/images/backdrops/backdrop9.jpg', 
   0, N'Coming Soon', 
   N'[2, 9, 8]', N'["Phiêu Lưu", "Giả Tưởng", "Gia Đình"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=yzb567', 
   N'Bùi Văn HH, Trần Thị II, Lý Văn JJ', 
   N'Hoàng Gia KK', 
   0, 1, 0,
   GETUTCDATE(), GETUTCDATE());

INSERT INTO Movies (TmdbId, Title, Overview, ReleaseDate, Duration, PosterPath, BackdropPath, VoteAverage, Status, 
                   GenreIds, GenreNames, Language, TrailerUrl, Cast, Director, IsNowShowing, IsComingSoon, IsFeatured,
                   CreatedAt, UpdatedAt)
VALUES 
(10, N'Tình Yêu Và Thời Gian', 
   N'Một câu chuyện tình yêu lãng mạn xuyên thời gian, khi một chàng trai phát hiện ra anh có thể du hành thời gian để gặp người yêu đã mất.', 
   '2023-12-30', 128, 
   N'/images/movies/movie10.jpg', 
   N'/images/backdrops/backdrop10.jpg', 
   0, N'Coming Soon', 
   N'[14, 15]', N'["Lãng Mạn", "Khoa Học Viễn Tưởng"]', 
   N'Vietnamese', 
   N'https://www.youtube.com/watch?v=cde890', 
   N'Phạm Văn LL, Nguyễn Thị MM, Cao Văn NN', 
   N'Lê Thái OO', 
   0, 1, 0,
   GETUTCDATE(), GETUTCDATE()); 