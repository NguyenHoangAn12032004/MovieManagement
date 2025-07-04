$(document).ready(function() {
    // Cập nhật preview vai trò được chọn
    function updateSelectedRolesPreview() {
        var selectedRoles = [];
        $('input[name="selectedRoles"]:checked').each(function() {
            selectedRoles.push($(this).val());
        });
        
        var previewContainer = $('#selected-roles-preview');
        if (selectedRoles.length > 0) {
            var html = '';
            selectedRoles.forEach(function(role) {
                html += '<span class="badge badge-info mr-2">' + role + '</span>';
            });
            previewContainer.html(html);
        } else {
            previewContainer.html('<span class="text-muted">Chưa chọn vai trò nào</span>');
        }
    }

    // Lắng nghe sự kiện thay đổi checkbox
    $('input[name="selectedRoles"]').change(function() {
        updateSelectedRolesPreview();
    });

    // Khởi tạo preview ban đầu
    updateSelectedRolesPreview();

    // Validate form trước khi submit
    $('#edit-user-form').submit(function(e) {
        var selectedRoles = $('input[name="selectedRoles"]:checked').length;
        
        if (selectedRoles === 0) {
            e.preventDefault();
            showAlert('warning', 'Vui lòng chọn ít nhất một vai trò cho tài khoản!');
            return false;
        }

        // Validate email
        var email = $('#Email').val();
        if (email && !isValidEmail(email)) {
            e.preventDefault();
            showAlert('danger', 'Email không đúng định dạng!');
            return false;
        }

        return true;
    });

    // Hàm validate email đơn giản
    function isValidEmail(email) {
        return email.indexOf('@') > 0 && email.indexOf('.') > 0;
    }

    // Hàm hiển thị alert
    function showAlert(type, message) {
        var alertHtml = '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
            message +
            '<button type="button" class="close" data-dismiss="alert">' +
            '<span>&times;</span>' +
            '</button>' +
            '</div>';
        
        $('#alert-container').html(alertHtml);
        
        // Auto hide sau 5 giây
        setTimeout(function() {
            $('.alert').alert('close');
        }, 5000);
    }
});
