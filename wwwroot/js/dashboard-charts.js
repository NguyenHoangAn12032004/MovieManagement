// Dashboard Charts - Simplified Version
console.log('Loading dashboard charts...');

// Wait for DOM and Chart.js to be ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM ready');
    
    // Check Chart.js availability
    if (typeof Chart === 'undefined') {
        console.error('Chart.js not loaded!');
        showChartError();
        return;
    }
    
    console.log('Chart.js version:', Chart.version);
    
    // Simple test first
    setTimeout(function() {
        console.log('Starting chart initialization...');
        initializeSimpleCharts();
    }, 100);
});

function showChartError() {
    document.querySelectorAll('.chart-wrapper').forEach(function(wrapper) {
        wrapper.innerHTML = '<div class="alert alert-warning text-center"><i class="fas fa-exclamation-triangle"></i><br>Chart.js not available</div>';
    });
}

function initializeSimpleCharts() {
    try {
        // Basic configuration
        Chart.defaults.responsive = true;
        Chart.defaults.maintainAspectRatio = false;
        
        // Create revenue chart
        const revenueCtx = document.getElementById('revenueChart');
        if (revenueCtx) {
            console.log('Creating revenue chart...');
            new Chart(revenueCtx, {
                type: 'line',
                data: {
                    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
                    datasets: [{
                        label: 'Revenue',
                        data: [120000, 150000, 200000, 175000, 250000, 300000, 280000],
                        borderColor: '#007bff',
                        backgroundColor: 'rgba(0, 123, 255, 0.1)',
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            display: true
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
            console.log('Revenue chart created successfully');
        }
        
        // Create movie chart
        const movieCtx = document.getElementById('movieChart');
        if (movieCtx) {
            console.log('Creating movie chart...');
            new Chart(movieCtx, {
                type: 'doughnut',
                data: {
                    labels: ['Movie A', 'Movie B', 'Movie C', 'Movie D'],
                    datasets: [{
                        data: [45, 35, 25, 20],
                        backgroundColor: ['#007bff', '#28a745', '#ffc107', '#dc3545']
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
            console.log('Movie chart created successfully');
        }
        
        // Create status chart
        const statusCtx = document.getElementById('bookingStatusChart');
        if (statusCtx) {
            console.log('Creating status chart...');
            new Chart(statusCtx, {
                type: 'doughnut',
                data: {
                    labels: ['Completed', 'Pending', 'Cancelled'],
                    datasets: [{
                        data: [80, 15, 5],
                        backgroundColor: ['#28a745', '#ffc107', '#dc3545']
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
            console.log('Status chart created successfully');
        }
        
        console.log('All charts initialized successfully');
        
    } catch (error) {
        console.error('Error initializing charts:', error);
    }
}
