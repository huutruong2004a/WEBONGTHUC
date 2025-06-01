// Enhanced Recipe Details JavaScript

document.addEventListener("DOMContentLoaded", () => {
    initializeRecipeDetails()
})

function initializeRecipeDetails() {
    // Initialize star rating
    initializeStarRating()

    // Initialize smooth scrolling
    initializeSmoothScrolling()

    // Initialize image lazy loading
    initializeImageLazyLoading()

    // Initialize review form validation
    initializeReviewForm()
}

// Star Rating Functionality
function initializeStarRating() {
    const ratingInputs = document.querySelectorAll(".rating-input input")
    const ratingLabels = document.querySelectorAll(".rating-input label")

    ratingLabels.forEach((label, index) => {
        label.addEventListener("mouseenter", () => {
            highlightStars(index)
        })

        label.addEventListener("click", () => {
            selectRating(index)
        })
    })

    // Reset on mouse leave
    const ratingContainer = document.querySelector(".rating-input")
    if (ratingContainer) {
        ratingContainer.addEventListener("mouseleave", () => {
            resetStarHighlight()
        })
    }
}

function highlightStars(index) {
    const labels = document.querySelectorAll(".rating-input label")
    labels.forEach((label, i) => {
        if (i >= index) {
            label.style.color = "#f59e0b"
        } else {
            label.style.color = "#d1d5db"
        }
    })
}

function selectRating(index) {
    const inputs = document.querySelectorAll(".rating-input input")
    const labels = document.querySelectorAll(".rating-input label")

    // Check the selected input
    inputs[index].checked = true

    // Update visual state
    labels.forEach((label, i) => {
        if (i >= index) {
            label.style.color = "#f59e0b"
        } else {
            label.style.color = "#d1d5db"
        }
    })
}

function resetStarHighlight() {
    const checkedInput = document.querySelector(".rating-input input:checked")
    const labels = document.querySelectorAll(".rating-input label")

    if (checkedInput) {
        const checkedIndex = Array.from(document.querySelectorAll(".rating-input input")).indexOf(checkedInput)
        labels.forEach((label, i) => {
            if (i >= checkedIndex) {
                label.style.color = "#f59e0b"
            } else {
                label.style.color = "#d1d5db"
            }
        })
    } else {
        labels.forEach((label) => {
            label.style.color = "#d1d5db"
        })
    }
}

// Smooth Scrolling
function initializeSmoothScrolling() {
    const links = document.querySelectorAll('a[href^="#"]')

    links.forEach((link) => {
        link.addEventListener("click", (e) => {
            e.preventDefault()
            const targetId = link.getAttribute("href").substring(1)
            const targetElement = document.getElementById(targetId)

            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: "smooth",
                    block: "start",
                })
            }
        })
    })
}

// Image Lazy Loading
function initializeImageLazyLoading() {
    const images = document.querySelectorAll("img[data-src]")

    if ("IntersectionObserver" in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    const img = entry.target
                    img.src = img.dataset.src
                    img.classList.remove("lazy")
                    imageObserver.unobserve(img)
                }
            })
        })

        images.forEach((img) => imageObserver.observe(img))
    } else {
        // Fallback for older browsers
        images.forEach((img) => {
            img.src = img.dataset.src
        })
    }
}

// Review Form Validation
function initializeReviewForm() {
    const reviewForm = document.querySelector(".review-form")

    if (reviewForm) {
        reviewForm.addEventListener("submit", (e) => {
            if (!validateReviewForm()) {
                e.preventDefault()
            }
        })
    }
}

function validateReviewForm() {
    const ratingInput = document.querySelector(".rating-input input:checked")
    const commentInput = document.querySelector('textarea[name="Comment"]')

    let isValid = true

    // Clear previous errors
    clearFormErrors()

    // Validate rating
    if (!ratingInput) {
        showFormError("rating", "Vui lòng chọn số sao đánh giá")
        isValid = false
    }

    // Validate comment
    if (!commentInput.value.trim()) {
        showFormError("comment", "Vui lòng nhập bình luận")
        isValid = false
    } else if (commentInput.value.trim().length < 10) {
        showFormError("comment", "Bình luận phải có ít nhất 10 ký tự")
        isValid = false
    }

    return isValid
}

function showFormError(field, message) {
    const errorElement = document.createElement("div")
    errorElement.className = "form-error"
    errorElement.textContent = message
    errorElement.style.cssText = `
    color: #dc2626;
    font-size: 0.875rem;
    margin-top: 0.25rem;
    font-weight: 500;
  `

    if (field === "rating") {
        const ratingContainer = document.querySelector(".rating-input")
        if (ratingContainer && !ratingContainer.nextElementSibling?.classList.contains("form-error")) {
            ratingContainer.parentNode.appendChild(errorElement)
        }
    } else if (field === "comment") {
        const commentInput = document.querySelector('textarea[name="Comment"]')
        if (commentInput && !commentInput.nextElementSibling?.classList.contains("form-error")) {
            commentInput.parentNode.appendChild(errorElement)
        }
    }
}

function clearFormErrors() {
    const errors = document.querySelectorAll(".form-error")
    errors.forEach((error) => error.remove())
}

// Utility Functions
function showNotification(message, type = "success") {
    const notification = document.createElement("div")
    notification.className = `notification ${type}`
    notification.textContent = message

    notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    padding: 1rem 1.5rem;
    border-radius: 0.5rem;
    color: white;
    font-weight: 600;
    z-index: 1000;
    background: ${type === "success"
            ? "linear-gradient(135deg, #10b981 0%, #059669 100%)"
            : "linear-gradient(135deg, #dc2626 0%, #b91c1c 100%)"
        };
    box-shadow: 0 8px 20px rgba(0, 0, 0, 0.2);
    transition: opacity 0.3s ease;
  `

    document.body.appendChild(notification)

    setTimeout(() => {
        notification.style.opacity = "0"
        setTimeout(() => notification.remove(), 300)
    }, 3000)
}

// Copy Recipe URL
function copyRecipeUrl() {
    const url = window.location.href

    if (navigator.clipboard) {
        navigator.clipboard
            .writeText(url)
            .then(() => {
                showNotification("Đã sao chép link công thức!")
            })
            .catch(() => {
                fallbackCopyTextToClipboard(url)
            })
    } else {
        fallbackCopyTextToClipboard(url)
    }
}

function fallbackCopyTextToClipboard(text) {
    const textArea = document.createElement("textarea")
    textArea.value = text
    textArea.style.position = "fixed"
    textArea.style.left = "-999999px"
    textArea.style.top = "-999999px"
    document.body.appendChild(textArea)
    textArea.focus()
    textArea.select()

    try {
        document.execCommand("copy")
        showNotification("Đã sao chép link công thức!")
    } catch (err) {
        showNotification("Không thể sao chép link", "error")
    }

    document.body.removeChild(textArea)
}

// Print Recipe
function printRecipe() {
    const printWindow = window.open("", "_blank")
    const recipeContent = document.querySelector(".main-content").innerHTML

    printWindow.document.write(`
    <!DOCTYPE html>
    <html>
    <head>
      <title>In công thức - ${document.title}</title>
      <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .content-card { margin-bottom: 20px; page-break-inside: avoid; }
        .card-header { border-bottom: 2px solid #eee; padding-bottom: 10px; margin-bottom: 15px; }
        .card-header h3 { margin: 0; color: #333; }
        .ingredient-item, .instruction-item { margin-bottom: 10px; }
        .recipe-image { max-width: 100%; height: auto; }
        @media print { body { margin: 0; } }
      </style>
    </head>
    <body>
      ${recipeContent}
    </body>
    </html>
  `)

    printWindow.document.close()
    printWindow.focus()

    setTimeout(() => {
        printWindow.print()
        printWindow.close()
    }, 250)
}

// Export functions for global use
window.copyRecipeUrl = copyRecipeUrl
window.printRecipe = printRecipe
