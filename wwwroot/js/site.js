// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Movie Carousel Functionality
document.addEventListener('DOMContentLoaded', function() {
    const carousel = document.querySelector('.movie-carousel');
    const prevBtn = document.getElementById('carousel-prev');
    const nextBtn = document.getElementById('carousel-next');
    const items = document.querySelectorAll('.movie-carousel-item');
    
    if (carousel && items.length > 0) {
        let currentIndex = 0;
        const itemWidth = items[0].offsetWidth + 20; // 20px is the gap
        const maxIndex = items.length - Math.floor(carousel.offsetWidth / itemWidth);

        function updateCarousel() {
            carousel.style.transform = `translateX(-${currentIndex * itemWidth}px)`;
            
            // Update button states
            prevBtn.style.opacity = currentIndex === 0 ? '0.5' : '1';
            nextBtn.style.opacity = currentIndex >= maxIndex ? '0.5' : '1';
        }

        prevBtn.addEventListener('click', () => {
            if (currentIndex > 0) {
                currentIndex--;
                updateCarousel();
            }
        });

        nextBtn.addEventListener('click', () => {
            if (currentIndex < maxIndex) {
                currentIndex++;
                updateCarousel();
            }
        });

        // Initialize button states
        updateCarousel();

        // Handle window resize
        let resizeTimer;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(() => {
                const newMaxIndex = items.length - Math.floor(carousel.offsetWidth / itemWidth);
                if (currentIndex > newMaxIndex) {
                    currentIndex = newMaxIndex;
                }
                updateCarousel();
            }, 250);
        });
    }
});
