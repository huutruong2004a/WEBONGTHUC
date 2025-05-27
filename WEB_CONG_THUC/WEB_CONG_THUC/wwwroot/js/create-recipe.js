// Complete Enhanced JavaScript for Recipe Form with Image Upload Fix

// Global variables
let ingredientCount = 1
let instructionCount = 1

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
    initializeEnhancements()
    addEventListeners()
    updateCounters()
    initializeImageUpload()
})

// Initialize all enhancements
function initializeEnhancements() {
    // Add loading animation to form
    const form = document.getElementById("recipeForm")
    if (form) {
        form.addEventListener("submit", (e) => {
            if (validateEnhancedForm()) {
                showLoadingState()
                // Allow form to submit after showing loading
                setTimeout(() => {
                    hideLoadingState()
                    showSuccessToast("Công thức đã được lưu thành công!")
                }, 1000)
            } else {
                e.preventDefault()
            }
        })
    }

    // Initialize counters
    updateCounters()

    // Add smooth scrolling to validation errors
    addSmoothScrollToErrors()

    // Initialize auto-save
    initializeAutoSave()
}

// Initialize image upload functionality
function initializeImageUpload() {
    // Test if file input works and add alternative if needed
    const fileInput = document.getElementById("ImageFile")
    if (fileInput) {
        // Add alternative button if needed
        setTimeout(addAlternativeUploadButton, 1000)
    }
}

// Add enhanced event listeners
function addEventListeners() {
    // Enhanced input focus effects
    document
        .querySelectorAll(".form-control, .form-input, .form-textarea, .form-select, .ingredient-input, .instruction-input")
        .forEach((input) => {
            input.addEventListener("focus", function () {
                this.closest(".form-group")?.classList.add("form-focused")
            })

            input.addEventListener("blur", function () {
                this.closest(".form-group")?.classList.remove("form-focused")
            })

            input.addEventListener("input", function () {
                clearFieldError(this)
                updateCounters()
            })
        })

    // Enhanced image preview with animation
    const imageInput = document.getElementById("ImageFile")
    if (imageInput) {
        imageInput.addEventListener("change", enhancedPreviewImage)
    }

    // Add keyboard shortcuts
    document.addEventListener("keydown", (e) => {
        // Ctrl/Cmd + S to save
        if ((e.ctrlKey || e.metaKey) && e.key === "s") {
            e.preventDefault()
            document.getElementById("recipeForm")?.submit()
        }

        // Ctrl/Cmd + Enter to add ingredient
        if ((e.ctrlKey || e.metaKey) && e.key === "Enter") {
            e.preventDefault()
            if (e.target.classList.contains("ingredient-input")) {
                addIngredient()
            } else if (e.target.classList.contains("instruction-input")) {
                addInstruction()
            }
        }
    })
}

// Function to trigger file input (for image upload fix)
function triggerFileInput() {
    const fileInput = document.getElementById("ImageFile")
    if (fileInput) {
        fileInput.click()
    }
}

// Enhanced image preview with animations and validation
function enhancedPreviewImage() {
    const input = document.getElementById("ImageFile")
    const preview = document.getElementById("imagePreview") || document.getElementById("preview")
    const container = preview?.closest(".image-preview-container") || preview?.closest(".image-preview")

    if (input.files && input.files[0]) {
        const file = input.files[0]

        // Validate file size (5MB max)
        if (file.size > 5 * 1024 * 1024) {
            showErrorToast("Kích thước file không được vượt quá 5MB")
            input.value = ""
            return
        }

        // Validate file type
        if (!file.type.startsWith("image/")) {
            showErrorToast("Vui lòng chọn file hình ảnh")
            input.value = ""
            return
        }

        const reader = new FileReader()

        reader.onload = (e) => {
            if (preview) {
                // Add loading effect
                preview.style.opacity = "0.5"
                container?.classList.add("loading")

                setTimeout(() => {
                    preview.src = e.target.result
                    preview.style.objectFit = "cover"
                    preview.style.opacity = "1"
                    container?.classList.remove("loading")

                    // Add success effect
                    if (container) {
                        container.style.borderColor = "#10b981"
                        container.style.background = "#f0fdf4"

                        setTimeout(() => {
                            container.style.borderColor = "#d1d5db"
                            container.style.background = "#f9fafb"
                        }, 2000)
                    }

                    showSuccessToast("Hình ảnh đã được tải lên thành công!")
                }, 300)
            }
        }

        reader.readAsDataURL(file)
    }
}

// Alternative: Add upload button if click doesn't work
function addAlternativeUploadButton() {
    const uploadArea = document.querySelector(".image-upload-area")
    if (uploadArea && !uploadArea.querySelector(".upload-button-alternative")) {
        const button = document.createElement("button")
        button.type = "button"
        button.className = "upload-button-alternative"
        button.innerHTML = `
            <i class="fas fa-upload"></i>
            Chọn hình ảnh
        `
        button.onclick = triggerFileInput

        // Add button styles
        button.style.cssText = `
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            background: linear-gradient(135deg, #f97316 0%, #dc2626 100%);
            color: white;
            border: none;
            border-radius: 0.5rem;
            cursor: pointer;
            font-weight: 600;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(249, 115, 22, 0.3);
            margin-top: 1rem;
            font-size: 0.875rem;
        `

        button.addEventListener("mouseenter", () => {
            button.style.transform = "translateY(-2px)"
            button.style.boxShadow = "0 8px 20px rgba(249, 115, 22, 0.4)"
        })

        button.addEventListener("mouseleave", () => {
            button.style.transform = "translateY(0)"
            button.style.boxShadow = "0 4px 12px rgba(249, 115, 22, 0.3)"
        })

        uploadArea.appendChild(button)
    }
}

// Enhanced add ingredient with animation
function addIngredient() {
    const list = document.getElementById("ingredientsList")
    if (!list) return

    ingredientCount++
    const newItem = document.createElement("div")
    newItem.className = "ingredient-item dynamic-item"
    newItem.style.opacity = "0"
    newItem.style.transform = "translateY(20px)"

    newItem.innerHTML = `
        <div class="item-number">${ingredientCount}</div>
        <input type="text" class="form-control form-input ingredient-input" 
               placeholder="Ví dụ: 200g bột mì" />
        <button type="button" class="btn-remove btn-remove-ingredient" onclick="removeIngredient(this)">
            <i class="fas fa-times"></i>
        </button>
    `

    list.appendChild(newItem)

    // Add event listeners to new elements
    const input = newItem.querySelector(".ingredient-input")
    const removeBtn = newItem.querySelector(".btn-remove-ingredient")

    input.addEventListener("input", () => {
        updateCounters()
        updateIngredientsValue_Create()
    })

    input.addEventListener("focus", function () {
        this.closest(".form-group")?.classList.add("form-focused")
    })

    input.addEventListener("blur", function () {
        this.closest(".form-group")?.classList.remove("form-focused")
    })

    removeBtn.addEventListener("click", function () {
        removeIngredientEnhanced(this)
    })

    // Animate in
    setTimeout(() => {
        newItem.style.transition = "all 0.3s ease"
        newItem.style.opacity = "1"
        newItem.style.transform = "translateY(0)"
        input.focus()
    }, 10)

    updateNumbering("ingredientsList")
    updateCounters()
    updateIngredientsValue_Create()
}

// Enhanced remove ingredient with animation
function removeIngredientEnhanced(button) {
    const item = button.closest(".ingredient-item") || button.closest(".dynamic-item")
    const list = document.getElementById("ingredientsList")

    if (list.children.length <= 1) {
        showWarningToast("Phải có ít nhất một nguyên liệu")
        return
    }

    // Animate out
    item.style.transition = "all 0.3s ease"
    item.style.opacity = "0"
    item.style.transform = "translateX(-20px)"

    setTimeout(() => {
        item.remove()
        updateNumbering("ingredientsList")
        updateCounters()
        updateIngredientsValue_Create()
    }, 300)
}

// Enhanced add instruction with animation
function addInstruction() {
    const list = document.getElementById("instructionsList")
    if (!list) return

    instructionCount++
    const stepNumber = list.children.length + 1
    const newItem = document.createElement("div")
    newItem.className = "instruction-item dynamic-item"
    newItem.style.opacity = "0"
    newItem.style.transform = "translateY(20px)"

    newItem.innerHTML = `
        <div class="item-number gradient-number">${stepNumber}</div>
        <div class="instruction-content">
            <textarea class="form-control form-textarea instruction-input" rows="3" 
                      placeholder="Mô tả chi tiết bước thực hiện..."></textarea>
            <button type="button" class="btn-remove-text btn-remove-instruction" onclick="removeInstruction(this)">
                <i class="fas fa-times"></i>
                Xóa bước
            </button>
        </div>
    `

    list.appendChild(newItem)

    // Add event listeners
    const textarea = newItem.querySelector(".instruction-input")
    const removeBtn = newItem.querySelector(".btn-remove-instruction")

    textarea.addEventListener("input", () => {
        updateCounters()
        updateInstructionsValue_Create()
    })

    textarea.addEventListener("focus", function () {
        this.closest(".form-group")?.classList.add("form-focused")
    })

    textarea.addEventListener("blur", function () {
        this.closest(".form-group")?.classList.remove("form-focused")
    })

    removeBtn.addEventListener("click", function () {
        removeInstructionEnhanced(this)
    })

    // Animate in
    setTimeout(() => {
        newItem.style.transition = "all 0.3s ease"
        newItem.style.opacity = "1"
        newItem.style.transform = "translateY(0)"
        textarea.focus()
    }, 10)

    updateInstructionNumbers_Create()
    updateCounters()
    updateInstructionsValue_Create()
}

// Enhanced remove instruction with animation
function removeInstructionEnhanced(button) {
    const item = button.closest(".instruction-item") || button.closest(".dynamic-item")
    const list = document.getElementById("instructionsList")

    if (list.children.length <= 1) {
        showWarningToast("Phải có ít nhất một bước thực hiện")
        return
    }

    // Animate out
    item.style.transition = "all 0.3s ease"
    item.style.opacity = "0"
    item.style.transform = "translateX(-20px)"

    setTimeout(() => {
        item.remove()
        updateInstructionNumbers_Create()
        updateCounters()
        updateInstructionsValue_Create()
    }, 300)
}

// Update numbering for dynamic lists
function updateNumbering(listId) {
    const list = document.getElementById(listId)
    const items = list.querySelectorAll(".dynamic-item")

    items.forEach((item, index) => {
        const numberElement = item.querySelector(".item-number")
        if (numberElement) {
            numberElement.textContent = index + 1
        }
    })
}

// Update counters with animation
function updateCounters() {
    const ingredientInputs = document.querySelectorAll(".ingredient-input")
    const instructionInputs = document.querySelectorAll(".instruction-input")

    const filledIngredients = Array.from(ingredientInputs).filter((input) => input.value.trim() !== "").length
    const filledInstructions = Array.from(instructionInputs).filter((input) => input.value.trim() !== "").length

    // Update ingredient counter
    const ingredientCounter =
        document.getElementById("ingredientCounter") ||
        document.querySelector(".ingredients-container .badge") ||
        document.querySelector(".item-counter")
    if (ingredientCounter) {
        ingredientCounter.textContent = `${filledIngredients} món`
        ingredientCounter.style.transform = "scale(1.1)"
        setTimeout(() => {
            ingredientCounter.style.transform = "scale(1)"
        }, 200)
    }

    // Update instruction counter
    const instructionCounter =
        document.getElementById("instructionCounter") ||
        document.querySelector(".instructions-container .badge") ||
        document.querySelectorAll(".item-counter")[1]
    if (instructionCounter) {
        instructionCounter.textContent = `${filledInstructions} bước`
        instructionCounter.style.transform = "scale(1.1)"
        setTimeout(() => {
            instructionCounter.style.transform = "scale(1)"
        }, 200)
    }
}

// Update hidden input values
function updateIngredientsValue_Create() {
    const inputs = document.querySelectorAll(".ingredient-input")
    const values = Array.from(inputs)
        .map((input) => input.value.trim())
        .filter((value) => value !== "")

    const hiddenInput = document.getElementById("ingredientsValue")
    if (hiddenInput) {
        hiddenInput.value = values.join("\n")
    }
}

function updateInstructionsValue_Create() {
    const textareas = document.querySelectorAll(".instruction-input")
    const values = Array.from(textareas)
        .map((textarea) => textarea.value.trim())
        .filter((value) => value !== "")

    const hiddenInput = document.getElementById("instructionsValue")
    if (hiddenInput) {
        hiddenInput.value = values.join("\n")
    }
}

function updateInstructionNumbers_Create() {
    updateNumbering("instructionsList")
}

// Enhanced form validation
function validateEnhancedForm() {
    let isValid = true
    const errors = []

    // Clear previous errors
    document.querySelectorAll(".text-danger, .validation-message").forEach((error) => {
        error.textContent = ""
    })

    // Validate required fields
    const title = document.getElementById("Title")
    if (title && !title.value.trim()) {
        showFieldError(title, "Tên công thức là bắt buộc")
        errors.push("title")
        isValid = false
    }

    const category = document.getElementById("CategoryId")
    if (category && !category.value) {
        showFieldError(category, "Vui lòng chọn danh mục")
        errors.push("category")
        isValid = false
    }

    // Validate ingredients
    const ingredientInputs = document.querySelectorAll(".ingredient-input")
    const hasIngredients = Array.from(ingredientInputs).some((input) => input.value.trim() !== "")

    if (!hasIngredients) {
        const ingredientsValue = document.getElementById("ingredientsValue")
        if (ingredientsValue) {
            showFieldError(ingredientsValue, "Phải có ít nhất một nguyên liệu")
        }
        errors.push("ingredients")
        isValid = false
    }

    // Validate instructions
    const instructionInputs = document.querySelectorAll(".instruction-input")
    const hasInstructions = Array.from(instructionInputs).some((input) => input.value.trim() !== "")

    if (!hasInstructions) {
        const instructionsValue = document.getElementById("instructionsValue")
        if (instructionsValue) {
            showFieldError(instructionsValue, "Phải có ít nhất một bước thực hiện")
        }
        errors.push("instructions")
        isValid = false
    }
    // Validate numeric fields
    ;["PrepTime", "CookTime", "Servings"].forEach((fieldId) => {
        const field = document.getElementById(fieldId)
        if (field && field.value) {
            const value = Number.parseInt(field.value)
            if (isNaN(value) || value < 1) {
                showFieldError(field, "Giá trị phải là số dương")
                errors.push(fieldId.toLowerCase())
                isValid = false
            }
        }
    })

    if (!isValid) {
        showErrorToast(`Vui lòng kiểm tra lại ${errors.length} trường bị lỗi`)
        scrollToFirstError()
    }

    return isValid
}

// Show field error with animation
function showFieldError(field, message) {
    const errorElement =
        field.parentNode.querySelector(".text-danger") || field.parentNode.querySelector(".validation-message")
    if (errorElement) {
        errorElement.textContent = message
        errorElement.style.opacity = "0"
        errorElement.style.transform = "translateY(-10px)"

        setTimeout(() => {
            errorElement.style.transition = "all 0.3s ease"
            errorElement.style.opacity = "1"
            errorElement.style.transform = "translateY(0)"
        }, 10)
    }

    field.classList.add("error")
    field.style.borderColor = "#dc2626"
    field.style.backgroundColor = "#fef2f2"
}

// Clear field error
function clearFieldError(field) {
    const errorElement =
        field.parentNode.querySelector(".text-danger") || field.parentNode.querySelector(".validation-message")
    if (errorElement) {
        errorElement.textContent = ""
    }

    field.classList.remove("error")
    field.style.borderColor = ""
    field.style.backgroundColor = ""
}

// Scroll to first error
function scrollToFirstError() {
    const firstError = document.querySelector(".text-danger:not(:empty), .validation-message:not(:empty)")
    if (firstError) {
        firstError.scrollIntoView({
            behavior: "smooth",
            block: "center",
        })
    }
}

// Loading states
function showLoadingState() {
    const form = document.getElementById("recipeForm")
    if (form) {
        form.classList.add("form-loading")
    }
}

function hideLoadingState() {
    const form = document.getElementById("recipeForm")
    if (form) {
        form.classList.remove("form-loading")
    }
}

// Toast notifications
function showSuccessToast(message) {
    showToast(message, "success")
}

function showErrorToast(message) {
    showToast(message, "error")
}

function showWarningToast(message) {
    showToast(message, "warning")
}

function showToast(message, type = "success") {
    // Remove existing toasts
    const existingToasts = document.querySelectorAll(".toast")
    existingToasts.forEach((toast) => toast.remove())

    const toast = document.createElement("div")
    toast.className = `toast ${type}`
    toast.innerHTML = `
        <i class="fas fa-${type === "success" ? "check-circle" : type === "error" ? "exclamation-circle" : "exclamation-triangle"}"></i>
        <span>${message}</span>
    `

    // Add toast styles
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${type === "success"
            ? "linear-gradient(135deg, #10b981 0%, #059669 100%)"
            : type === "error"
                ? "linear-gradient(135deg, #dc2626 0%, #b91c1c 100%)"
                : "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)"
        };
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 0.5rem;
        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
        display: flex;
        align-items: center;
        gap: 0.5rem;
        z-index: 1000;
        font-weight: 600;
        animation: slideInRight 0.3s ease;
    `

    document.body.appendChild(toast)

    setTimeout(() => {
        toast.style.animation = "slideOutRight 0.3s ease"
        setTimeout(() => toast.remove(), 300)
    }, 3000)
}

// Auto-save functionality
function initializeAutoSave() {
    let autoSaveTimer

    function autoSave() {
        clearTimeout(autoSaveTimer)
        autoSaveTimer = setTimeout(() => {
            const formData = new FormData(document.getElementById("recipeForm"))
            const data = Object.fromEntries(formData)
            localStorage.setItem("recipeFormDraft", JSON.stringify(data))

            // Show subtle save indicator
            const indicator = document.createElement("div")
            indicator.textContent = "Đã lưu tự động"
            indicator.style.cssText = `
                position: fixed;
                bottom: 20px;
                right: 20px;
                background: #10b981;
                color: white;
                padding: 0.5rem 1rem;
                border-radius: 0.25rem;
                font-size: 0.875rem;
                z-index: 1000;
                opacity: 0;
                transition: opacity 0.3s ease;
            `

            document.body.appendChild(indicator)
            setTimeout(() => (indicator.style.opacity = "1"), 10)
            setTimeout(() => {
                indicator.style.opacity = "0"
                setTimeout(() => indicator.remove(), 300)
            }, 2000)
        }, 2000)
    }

    // Add auto-save listeners
    document.addEventListener("input", autoSave)
}

// Smooth scroll to errors
function addSmoothScrollToErrors() {
    document.addEventListener(
        "invalid",
        (e) => {
            e.target.scrollIntoView({
                behavior: "smooth",
                block: "center",
            })
        },
        true,
    )
}

// Enhanced back button
function goBack() {
    if (confirm("Bạn có chắc muốn quay lại? Dữ liệu chưa lưu sẽ bị mất.")) {
        window.history.back()
    }
}

// Global functions for compatibility
window.previewImage = enhancedPreviewImage
window.addIngredient = addIngredient
window.addInstruction = addInstruction
window.removeIngredient = removeIngredientEnhanced
window.removeInstruction = removeInstructionEnhanced
window.triggerFileInput = triggerFileInput
window.goBack = goBack

// Add CSS animations
const style = document.createElement("style")
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`
document.head.appendChild(style)
