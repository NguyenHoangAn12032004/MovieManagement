window.addEventListener('DOMContentLoaded', event => {
    // Toggle the side navigation
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
        });
    }

    // Add active class to current nav item
    const path = window.location.pathname;
    const navLinks = document.querySelectorAll('.sb-sidenav-menu .nav-link');
    navLinks.forEach(link => {
        if (link.getAttribute('href') === path) {
            link.classList.add('active');
        }
    });

    // Initialize all DataTables
    const tables = document.querySelectorAll('.datatable');
    tables.forEach(table => {
        new DataTable(table, {
            responsive: true,
            language: {
                search: "Tìm kiếm:",
                lengthMenu: "Hiển thị _MENU_ mục",
                info: "Hiển thị _START_ đến _END_ của _TOTAL_ mục",
                infoEmpty: "Hiển thị 0 đến 0 của 0 mục",
                infoFiltered: "(được lọc từ _MAX_ mục)",
                zeroRecords: "Không tìm thấy kết quả phù hợp",
                emptyTable: "Không có dữ liệu",
                paginate: {
                    first: "Đầu",
                    previous: "Trước",
                    next: "Tiếp",
                    last: "Cuối"
                }
            }
        });
    });
}); 