// Student Profile JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Handle image errors
    const handleImageError = (img) => {
        img.src = '/assets/images/default-avatar.png';
        img.alt = 'Avatar mặc định';
    };

    // Attach error handlers to all profile images
    const profileImages = document.querySelectorAll('.profile-card img');
    profileImages.forEach(img => {
        img.addEventListener('error', () => handleImageError(img));
    });

    // Tab functionality
    const triggerTabList = document.querySelectorAll('#myTab button');
    triggerTabList.forEach(triggerEl => {
        const tabTrigger = new bootstrap.Tab(triggerEl);
        triggerEl.addEventListener('click', event => {
            event.preventDefault();
            tabTrigger.show();
        });
    });

    // Print profile functionality
    const printProfile = () => {
        window.print();
    };

    // Export functionality
    const exportProfile = () => {
        // Implementation for exporting profile data
        console.log('Exporting profile data...');
    };

    // Attach event listeners if buttons exist
    const printBtn = document.getElementById('printProfile');
    const exportBtn = document.getElementById('exportProfile');
    
    if (printBtn) printBtn.addEventListener('click', printProfile);
    if (exportBtn) exportBtn.addEventListener('click', exportProfile);
});