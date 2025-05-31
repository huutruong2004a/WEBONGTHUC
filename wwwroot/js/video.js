document.addEventListener('DOMContentLoaded', function () {
    // Xử lý yêu thích video
    document.querySelectorAll('.btn-favorite').forEach(button => {
        button.addEventListener('click', async function () {
            if (!isAuthenticated) {
                window.location.href = '/Identity/Account/Login';
                return;
            }

            const videoId = this.dataset.videoId;
            try {
                const response = await fetch(`/Videos/ToggleFavorite/${videoId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });

                if (response.ok) {
                    this.classList.toggle('active');
                }
            } catch (error) {
                console.error('Error:', error);
            }
        });
    });

    // Xử lý tìm kiếm và lọc
    const searchInput = document.getElementById('searchVideo');
    const recipeFilter = document.getElementById('recipeFilter');

    function filterVideos() {
        const searchValue = searchInput.value.toLowerCase();
        const recipeValue = recipeFilter.value;

        document.querySelectorAll('.video-card').forEach(card => {
            const title = card.querySelector('h3').textContent.toLowerCase();
            const recipeId = card.dataset.recipeId;

            const matchesSearch = title.includes(searchValue);
            const matchesRecipe = !recipeValue || recipeId === recipeValue;

            card.style.display = matchesSearch && matchesRecipe ? 'block' : 'none';
        });
    }

    if (searchInput) {
        searchInput.addEventListener('input', filterVideos);
    }

    if (recipeFilter) {
        recipeFilter.addEventListener('change', filterVideos);
    }
});