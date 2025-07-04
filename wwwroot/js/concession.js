// Concession selection functionality
document.addEventListener('DOMContentLoaded', function() {
    console.log('Concession JS loaded - DOM ready');
    
    // Fallback if jQuery is not available
    if (typeof $ === 'undefined') {
        console.error('jQuery not loaded!');
        return;
    }
    
    console.log('jQuery available, setting up concession functionality');
    
    // Test elements availability
    setTimeout(function() {
        const quantityInputs = document.querySelectorAll('.quantity-input');
        const cartItemsList = document.getElementById('cartItemsList');
        const orderButton = document.getElementById('orderButton');
        
        console.log('Elements found:', {
            quantityInputs: quantityInputs.length,
            cartItemsList: cartItemsList ? 'YES' : 'NO',
            orderButton: orderButton ? 'YES' : 'NO'
        });
        
        // Call initial update
        updateTotalPrice();
    }, 100);
    
    // Handle quantity increase button with vanilla JS as fallback
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('btn-increase')) {
            e.preventDefault();
            console.log('Increase button clicked');
            
            const input = e.target.parentElement.querySelector('.quantity-input');
            if (input) {
                const currentVal = parseInt(input.value) || 0;
                if (currentVal < 10) {
                    input.value = currentVal + 1;
                    console.log('New value:', currentVal + 1);
                    updateTotalPrice();
                    
                    // Add pulse effect
                    const card = e.target.closest('.concession-card');
                    if (card) {
                        card.classList.add('pulse');
                        setTimeout(() => card.classList.remove('pulse'), 500);
                    }
                }
            }
        }
    });

    // Handle quantity decrease button
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('btn-decrease')) {
            e.preventDefault();
            console.log('Decrease button clicked');
            
            const input = e.target.parentElement.querySelector('.quantity-input');
            if (input) {
                const currentVal = parseInt(input.value) || 0;
                if (currentVal > 0) {
                    input.value = currentVal - 1;
                    console.log('New value:', currentVal - 1);
                    updateTotalPrice();
                }
            }
        }
    });

    // Handle direct input changes
    document.addEventListener('change', function(e) {
        if (e.target.classList.contains('quantity-input')) {
            console.log('Input changed directly');
            updateTotalPrice();
        }
    });    // Update total price and cart display (Vanilla JS + jQuery fallback)
    function updateTotalPrice() {
        console.log('=== updateTotalPrice START ===');
        
        let totalPrice = 0;
        let totalItems = 0;
        let discount = 0;
        
        // Get elements (vanilla JS first, jQuery fallback)
        const cartItemsList = document.getElementById('cartItemsList') || ($ ? $('#cartItemsList')[0] : null);
        const emptyCart = document.getElementById('emptyCart') || ($ ? $('#emptyCart')[0] : null);
        const subtotalRow = document.getElementById('subtotalRow') || ($ ? $('#subtotalRow')[0] : null);
        const discountRow = document.getElementById('discountRow') || ($ ? $('#discountRow')[0] : null);
        const totalRow = document.getElementById('totalRow') || ($ ? $('#totalRow')[0] : null);
        const orderButton = document.getElementById('orderButton') || ($ ? $('#orderButton')[0] : null);
        
        if (!cartItemsList) {
            console.error('Cart items list not found!');
            return;
        }
        
        console.log('Elements found:', {
            cartItemsList: !!cartItemsList,
            emptyCart: !!emptyCart,
            subtotalRow: !!subtotalRow,
            totalRow: !!totalRow,
            orderButton: !!orderButton
        });
        
        // Clear existing cart items (except empty cart row)
        const existingRows = cartItemsList.querySelectorAll('tr:not(#emptyCart)');
        existingRows.forEach(row => row.remove());
        
        // Get all quantity inputs
        const quantityInputs = document.querySelectorAll('.quantity-input');
        console.log('Found quantity inputs:', quantityInputs.length);
        
        // Process each item
        quantityInputs.forEach((input, index) => {
            const quantity = parseInt(input.value) || 0;
            const price = parseFloat(input.dataset.price) || 0;
            const name = input.dataset.name || 'Unknown';
            
            console.log(`Item ${index}:`, {
                name: name,
                quantity: quantity,
                price: price
            });
            
            if (quantity > 0) {
                const itemTotal = price * quantity;
                totalPrice += itemTotal;
                totalItems += quantity;
                
                // Add row to cart table
                const cartRow = document.createElement('tr');
                cartRow.innerHTML = `
                    <td>${name}</td>
                    <td class="text-center">${quantity}</td>
                    <td class="text-end">${price.toLocaleString('vi-VN')} đ</td>
                    <td class="text-end">${itemTotal.toLocaleString('vi-VN')} đ</td>
                `;
                cartItemsList.appendChild(cartRow);
                
                console.log('Added cart row for:', name);
            }
        });
        
        console.log('Totals:', { totalItems, totalPrice });        
        // Apply discount if combo with ticket
        const isComboWithTicket = document.querySelector('.alert-success') && 
                                 document.querySelector('.alert-success').textContent.includes('Mua kèm vé');
        if (isComboWithTicket && totalPrice > 0) {
            discount = totalPrice * 0.05;
        }
        
        console.log('Discount:', discount, 'isComboWithTicket:', isComboWithTicket);
        
        // Update UI
        if (totalItems > 0) {
            console.log('Showing cart items');
            
            // Hide empty cart message
            if (emptyCart) emptyCart.style.display = 'none';
            
            // Show cart summary rows
            if (subtotalRow) subtotalRow.classList.remove('d-none');
            if (totalRow) totalRow.classList.remove('d-none');
            
            // Update subtotal
            const subtotalElement = document.querySelector('#subtotal');
            if (subtotalElement) {
                subtotalElement.textContent = totalPrice.toLocaleString('vi-VN') + ' đ';
            }
            
            // Update discount and total
            if (discount > 0) {
                if (discountRow) discountRow.classList.remove('d-none');
                const discountElement = document.querySelector('#discount');
                if (discountElement) {
                    discountElement.textContent = '- ' + discount.toLocaleString('vi-VN') + ' đ';
                }
                const totalElement = document.querySelector('#total');
                if (totalElement) {
                    totalElement.textContent = (totalPrice - discount).toLocaleString('vi-VN') + ' đ';
                }
            } else {
                if (discountRow) discountRow.classList.add('d-none');
                const totalElement = document.querySelector('#total');
                if (totalElement) {
                    totalElement.textContent = totalPrice.toLocaleString('vi-VN') + ' đ';
                }
            }
        } else {
            console.log('Hiding cart items');
            
            // Show empty cart message
            if (emptyCart) emptyCart.style.display = '';
            
            // Hide cart summary rows
            if (subtotalRow) subtotalRow.classList.add('d-none');
            if (discountRow) discountRow.classList.add('d-none');
            if (totalRow) totalRow.classList.add('d-none');
        }
        
        // Enable/disable order button based on whether we have tickets or concession items
        const hasTickets = document.querySelector('input[name="showtimeId"]') && 
                          document.querySelector('input[name="selectedSeats"]');
        const hasConcessionItems = totalItems > 0;
        
        console.log('Button state:', { hasTickets: !!hasTickets, hasConcessionItems });
        
        if (orderButton) {
            if (hasTickets || hasConcessionItems) {
                orderButton.disabled = false;
                if (hasTickets && !hasConcessionItems) {
                    orderButton.innerHTML = '<i class="fas fa-shopping-cart me-2"></i>Thêm vé vào giỏ hàng';
                } else if (!hasTickets && hasConcessionItems) {
                    orderButton.innerHTML = '<i class="fas fa-shopping-cart me-2"></i>Thêm bắp nước vào giỏ hàng';
                } else {
                    orderButton.innerHTML = '<i class="fas fa-shopping-cart me-2"></i>Thêm vào giỏ hàng';
                }
            } else {
                orderButton.disabled = true;
                orderButton.innerHTML = '<i class="fas fa-shopping-cart me-2"></i>Thêm vào giỏ hàng';
            }
        }
          console.log('=== updateTotalPrice END ===');
    }

    // Filter out zero quantity items before form submission  
    const concessionForm = document.getElementById('concessionForm');
    if (concessionForm) {
        concessionForm.addEventListener('submit', function() {
            console.log('Form submitted');
            // Remove items with zero quantity from submission
            const quantityInputs = document.querySelectorAll('.quantity-input');
            quantityInputs.forEach(function(input) {
                if (parseInt(input.value) === 0) {
                    // Get the associated hidden item ID input
                    const hiddenInput = input.parentNode.querySelector('.item-id-input');
                    if (hiddenInput) {
                        hiddenInput.remove(); // Remove instead of disable to avoid sending empty values
                    }
                    input.remove();
                }
            });
            
            // Allow submission even if no concession items are selected
            // because user might just want to buy tickets only
            return true;
        });
    }

    // Load initial state and test
    console.log('Testing elements on page load:');
    console.log('quantity-input elements:', document.querySelectorAll('.quantity-input').length);
    console.log('cartItemsList element:', document.getElementById('cartItemsList') ? 1 : 0);
    console.log('orderButton element:', document.getElementById('orderButton') ? 1 : 0);
    
    // Call updateTotalPrice initially
    updateTotalPrice();
    
    // Test manual call (you can remove this later)
    window.testUpdatePrice = updateTotalPrice;
    
    console.log('Concession JS setup complete');
});

// Fallback function using vanilla JS if jQuery fails
function fallbackUpdatePrice() {
    console.log('Using fallback vanilla JS');
    
    // Simple fallback implementation
    const quantityInputs = document.querySelectorAll('.quantity-input');
    const orderButton = document.getElementById('orderButton');
    
    let hasItems = false;
    quantityInputs.forEach(function(input) {
        if (parseInt(input.value) > 0) {
            hasItems = true;
        }
    });
    
    if (orderButton) {
        orderButton.disabled = !hasItems;
    }
    
    console.log('Fallback complete, hasItems:', hasItems);
}
