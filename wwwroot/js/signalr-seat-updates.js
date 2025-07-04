// SignalR client-side code for seat updates
document.addEventListener('DOMContentLoaded', function() {
    // Check if we're on the seat selection page
    const seatElements = document.querySelectorAll('.seat');
    if (seatElements.length === 0) return;
    
    // Lấy thông tin showtimeId từ URL để debug
    const urlParams = new URLSearchParams(window.location.search);
    const currentShowtimeId = urlParams.get('showtimeId');
    console.log(`Current page showtimeId: ${currentShowtimeId}`);
    
    // Connect to SignalR hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/seathub")
        .withAutomaticReconnect()
        .build();

    // Start the connection
    connection.start().then(function() {
        console.log(`SignalR connected. Ready to receive updates for seats on showtimeId=${currentShowtimeId}`);
    }).catch(function(err) {
        console.error("Error connecting to SignalR hub:", err);
    });

    // Listen for seat updates from admin
    connection.on("ReceiveSeatUpdate", function(seatId, row, number, status, isAvailable) {
        console.log(`Received update for seat ${row}${number} (ID=${seatId}): status=${status}, isAvailable=${isAvailable ? 'true' : 'false'}`);
        
        // In thêm thông tin debug
        const seatPosition = `${row}${number}`;
        let infoLog = `Looking for seat ${seatPosition} on page with showtimeId=${currentShowtimeId}.\n`;
        infoLog += `Received attributes: seatId=${seatId}, status=${status}, isAvailable=${isAvailable}\n`;
        
        // Tìm tất cả ghế có cùng row và number trên trang hiện tại
        const seatElements = [];
        
        // Phương pháp 1: Tìm theo data-seat attribute (cách chính xác nhất)
        const dataSeats = document.querySelectorAll(`.seat[data-seat="${seatPosition}"]`);
        if (dataSeats.length > 0) {
            infoLog += `Found ${dataSeats.length} seats using [data-seat="${seatPosition}"] selector\n`;
            dataSeats.forEach(s => seatElements.push(s));
        } else {
            infoLog += `No seats found using [data-seat="${seatPosition}"] selector\n`;
        }
        
        // Phương pháp 2: Tìm theo cách thủ công (backup)
        if (dataSeats.length === 0) {
            // Tìm tất cả các row có row-label đúng
            const rowLabels = document.querySelectorAll('.row-label');
            rowLabels.forEach(rowLabel => {
                if (rowLabel.textContent.trim() === row) {
                    // Tìm hàng ghế (seat-row)
                    const seatRow = rowLabel.closest('.seat-row');
                    if (seatRow) {
                        // Tìm tất cả ghế trong hàng này
                        seatRow.querySelectorAll('.seat').forEach(seatElement => {
                            // Kiểm tra nếu số ghế khớp
                            if (seatElement.textContent.trim() == number) {
                                infoLog += `Found seat using DOM traversal in row ${row}\n`;
                                seatElements.push(seatElement);
                            }
                        });
                    }
                }
            });
        }
        
        console.log(infoLog);
        console.log(`Total seats found on this page: ${seatElements.length}`);
        
        if (seatElements.length > 0) {
            // Cập nhật tất cả các ghế tìm thấy
            seatElements.forEach(seatElement => {
                // Lưu trữ id của ghế cho debug
                const elementSeatId = seatElement.getAttribute('data-seat-id');
                console.log(`Updating seat element with data-seat-id=${elementSeatId}`);
                
                // Kiểm tra nếu ghế đang được chọn
                const isCurrentlySelected = seatElement.classList.contains('selected');
                
                // Cập nhật các thuộc tính
                seatElement.setAttribute('data-available', isAvailable.toString().toLowerCase());
                seatElement.setAttribute('data-status', status);
                
                // Xóa tất cả class trạng thái (giữ lại selected nếu cần)
                const classesToRemove = ['standard', 'vip', 'couple', 'unavailable', 'maintenance', 'cleaning'];
                seatElement.classList.remove(...classesToRemove);
                
                // Thêm class mới dựa trên trạng thái
                if (!isAvailable) {
                    // Nếu ghế đang được chọn nhưng bây giờ không khả dụng, hiển thị thông báo
                    if (isCurrentlySelected) {
                        // Xóa khỏi danh sách đã chọn trước
                        const seatId = seatElement.getAttribute('data-seat-id');
                        seatElement.classList.remove('selected');
                        
                        if (window.selectedSeats && window.selectedSeats.has(seatId)) {
                            window.selectedSeats.delete(seatId);
                            if (typeof window.updateBookingInfo === 'function') {
                                window.updateBookingInfo();
                            }
                            
                            // Hiển thị thông báo
                            alert(`Ghế ${seatPosition} vừa được cập nhật trạng thái thành "${status}" và không còn khả dụng!`);
                        }
                    }
                    
                    if (status === 'Maintenance') {
                        seatElement.classList.add('maintenance');
                    } else if (status === 'Cleaning') {
                        seatElement.classList.add('cleaning');
                    } else {
                        seatElement.classList.add('unavailable');
                    }
                    
                    // Cập nhật tooltip
                    if (typeof bootstrap !== 'undefined') {
                        const tooltip = bootstrap.Tooltip.getInstance(seatElement);
                        if (tooltip) {
                            tooltip.dispose();
                        }
                        
                        let tooltipText = 'Ghế không khả dụng';
                        if (status === 'Maintenance') {
                            tooltipText = 'Ghế đang bảo trì';
                        } else if (status === 'Cleaning') {
                            tooltipText = 'Ghế đang vệ sinh';
                        } else if (!isAvailable) {
                            tooltipText = 'Ghế đã được đặt';
                        }
                        
                        seatElement.setAttribute('title', tooltipText);
                        new bootstrap.Tooltip(seatElement);
                    }
                    
                    // Vô hiệu hóa khả năng click vào ghế
                    seatElement.style.pointerEvents = 'none';
                } else {
                    // Nếu ghế có sẵn, thêm lại class type tương ứng
                    const seatType = seatElement.getAttribute('data-type');
                    if (seatType) {
                        seatElement.classList.add(seatType.toLowerCase());
                    } else {
                        // Mặc định thêm class standard nếu không có data-type
                        seatElement.classList.add('standard');
                    }
                    
                    // Khôi phục khả năng click vào ghế
                    seatElement.style.pointerEvents = 'auto';
                    
                    // Thêm lại class selected nếu ghế này đang được chọn
                    if (isCurrentlySelected) {
                        seatElement.classList.add('selected');
                    }
                }
                
                // Thêm hiệu ứng highlight tạm thời khi ghế được cập nhật
                seatElement.style.transition = 'all 0.3s';
                seatElement.style.boxShadow = '0 0 10px 2px yellow';
                
                setTimeout(function() {
                    seatElement.style.boxShadow = '';
                }, 2000);
                
                console.log(`Seat ${seatPosition} updated successfully, current classes: ${seatElement.classList}`);
            });
        } else {
            console.warn(`Không tìm thấy ghế nào có vị trí ${seatPosition} trên trang với showtimeId=${currentShowtimeId}`);
            
            // In toàn bộ ghế trên trang để debug
            const allSeats = [];
            document.querySelectorAll('.seat').forEach(seat => {
                const dataSeat = seat.getAttribute('data-seat');
                const seatRow = seat.closest('.seat-row');
                const rowLabel = seatRow ? seatRow.querySelector('.row-label')?.textContent.trim() : 'unknown';
                const seatNumber = seat.textContent.trim();
                
                allSeats.push({
                    'data-seat': dataSeat,
                    'row': rowLabel,
                    'number': seatNumber
                });
            });
            
            console.log('Available seats on this page:', allSeats);
        }
    });
}); 