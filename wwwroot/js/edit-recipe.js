
let ingredientCount = 0
let instructionCount = 0

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
    initializeForm()
    loadExistingData()
    updateCounters()
})

function initializeForm() {
    // Form submission
    const form = document.getElementById("recipeForm")
    if (form) {
        form.addEventListener("submit", (e) => {
            if (validateForm()) {
                updateIngredientsValue()
                updateInstructionsValue()
                showSuccessMessage("Đang lưu thay đổi...")
            } else {
                e.preventDefault()
            }
        })
    }

    // Add event listeners for inputs
    document.addEventListener("input", (e) => {
        if (e.target.classList.contains("ingredient-input")) {
            updateIngredientsValue()
            updateCounters()
        }
        if (e.target.classList.contains("instruction-input")) {
            updateInstructionsValue()
            updateCounters()
        }
    })
}

// Load existing data from model
function loadExistingData() {
    loadIngredients()
    loadInstructions()
}

// Image preview functionality
function previewImage() {
    const input = document.getElementById("ImageFile")
    const preview = document.getElementById("imagePreview")

    if (input.files && input.files[0]) {
        const file = input.files[0]

        // Validate file
        if (file.size > 5 * 1024 * 1024) {
            showErrorMessage("Kích thước file không được vượt quá 5MB")
            input.value = ""
            return
        }

        if (!file.type.startsWith("image/")) {
            showErrorMessage("Vui lòng chọn file hình ảnh")
            input.value = ""
            return
        }

        const reader = new FileReader()
        reader.onload = (e) => {
            preview.src = e.target.result
            showSuccessMessage("Hình ảnh đã được cập nhật!")
        }
        reader.readAsDataURL(file)
    }
}

// Trigger file input
function triggerFileInput() {
    const fileInput = document.getElementById("ImageFile")
    if (fileInput) {
        fileInput.click()
    }
}

// Load ingredients from model
function loadIngredients() {
    const ingredientsValue = document.getElementById("ingredientsValue").value.trim()
    const list = document.getElementById("ingredientsList")
    list.innerHTML = ""

    if (ingredientsValue) {
        const items = ingredientsValue.split("\n")
        items.forEach((item) => {
            if (item.trim()) {
                addIngredientItem(item.trim())
            }
        })
    }

    // Add empty item if no ingredients
    if (list.children.length === 0) {
        addIngredientItem("")
    }

    updateCounters()
}

// Load instructions from model
function loadInstructions() {
    const instructionsValue = document.getElementById("instructionsValue").value.trim()
    const list = document.getElementById("instructionsList")
    list.innerHTML = ""

    if (instructionsValue) {
        const items = instructionsValue.split("\n")
        items.forEach((item) => {
            if (item.trim()) {
                addInstructionItem(item.trim())
            }
        })
    }

    // Add empty item if no instructions
    if (list.children.length === 0) {
        addInstructionItem("")
    }

    updateCounters()
}

// Add ingredient item
function addIngredientItem(value = "") {
    const list = document.getElementById("ingredientsList")
    ingredientCount++

    const newItem = document.createElement("div")
    newItem.className = "dynamic-item"
    newItem.innerHTML = `
        <div class="item-number">${ingredientCount}</div>
        <input type="text" class="form-input ingredient-input" 
               placeholder="Ví dụ: 200g bột mì" value="${value.replace(/"/g, "&quot;")}">
        <button type="button" class="btn-remove" onclick="removeIngredient(this)">
            <i class="fas fa-times"></i>
        </button>
    `

    list.appendChild(newItem)
    updateNumbering("ingredientsList")
}

// Add instruction item
function addInstructionItem(value = "") {
    const list = document.getElementById("instructionsList")
    instructionCount++

    const newItem = document.createElement("div")
    newItem.className = "dynamic-item instruction-item"
    newItem.innerHTML = `
        <div class="item-number gradient-number">${instructionCount}</div>
        <div class="instruction-content">
            <textarea class="form-textarea instruction-input" rows="3" 
                    placeholder="Mô tả chi tiết bước thực hiện...">${value.replace(/"/g, "&quot;")}</textarea>
            <button type="button" class="btn-remove-text" onclick="removeInstruction(this)">
                <i class="fas fa-times"></i>
                Xóa bước
            </button>
        </div>
    `

    list.appendChild(newItem)
    updateNumbering("instructionsList")
}

// Add ingredient (called by button)
function addIngredient() {
    addIngredientItem("")
    updateIngredientsValue()
    updateCounters()

    // Focus on new input
    const list = document.getElementById("ingredientsList")
    const newInput = list.lastElementChild.querySelector(".ingredient-input")
    newInput.focus()
}

// Add instruction (called by button)
function addInstruction() {
    addInstructionItem("")
    updateInstructionsValue()
    updateCounters()

    // Focus on new textarea
    const list = document.getElementById("instructionsList")
    const newTextarea = list.lastElementChild.querySelector(".instruction-input")
    newTextarea.focus()
}

// Remove ingredient
function removeIngredient(button) {
    const item = button.closest(".dynamic-item")
    const list = document.getElementById("ingredientsList")

    if (list.children.length > 1) {
        item.remove()
        updateNumbering("ingredientsList")
        updateIngredientsValue()
        updateCounters()
    } else {
        showWarningMessage("Phải có ít nhất một nguyên liệu")
    }
}

// Remove instruction
function removeInstruction(button) {
    const item = button.closest(".dynamic-item")
    const list = document.getElementById("instructionsList")

    if (list.children.length > 1) {
        item.remove()
        updateNumbering("instructionsList")
        updateInstructionsValue()
        updateCounters()
    } else {
        showWarningMessage("Phải có ít nhất một bước thực hiện")
    }
}

// Update numbering
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

// Update hidden input values
function updateIngredientsValue() {
    const inputs = document.querySelectorAll(".ingredient-input")
    const values = Array.from(inputs)
        .map((input) => input.value.trim())
        .filter((value) => value !== "")

    document.getElementById("ingredientsValue").value = values.join("\n")
}

function updateInstructionsValue() {
    const textareas = document.querySelectorAll(".instruction-input")
    const values = Array.from(textareas)
        .map((textarea) => textarea.value.trim())
        .filter((value) => value !== "")

    document.getElementById("instructionsValue").value = values.join("\n")
}

// Update counters
function updateCounters() {
    const ingredientInputs = document.querySelectorAll(".ingredient-input")
    const instructionInputs = document.querySelectorAll(".instruction-input")

    const filledIngredients = Array.from(ingredientInputs).filter((input) => input.value.trim() !== "").length
    const filledInstructions = Array.from(instructionInputs).filter((input) => input.value.trim() !== "").length

    const ingredientCounter = document.getElementById("ingredientCounter")
    const instructionCounter = document.getElementById("instructionCounter")

    if (ingredientCounter) {
        ingredientCounter.textContent = `${filledIngredients} món`
        // Minimal animation
        ingredientCounter.style.transform = "scale(1.05)"
        setTimeout(() => {
            ingredientCounter.style.transform = "scale(1)"
        }, 150)
    }

    if (instructionCounter) {
        instructionCounter.textContent = `${filledInstructions} bước`
        // Minimal animation
        instructionCounter.style.transform = "scale(1.05)"
        setTimeout(() => {
            instructionCounter.style.transform = "scale(1)"
        }, 150)
    }
}

// Form validation
function validateForm() {
    let isValid = true

    // Clear previous errors
    document.querySelectorAll(".validation-message").forEach((error) => {
        error.textContent = ""
    })

    // Validate required fields
    const title = document.getElementById("Title")
    if (title && !title.value.trim()) {
        showFieldError(title, "Tên công thức là bắt buộc")
        isValid = false
    }

    const category = document.getElementById("CategoryId")
    if (category && !category.value) {
        showFieldError(category, "Vui lòng chọn danh mục")
        isValid = false
    }

    // Validate ingredients
    const ingredientInputs = document.querySelectorAll(".ingredient-input")
    const hasIngredients = Array.from(ingredientInputs).some((input) => input.value.trim() !== "")

    if (!hasIngredients) {
        showFieldError(document.getElementById("ingredientsValue"), "Phải có ít nhất một nguyên liệu")
        isValid = false
    }

    // Validate instructions
    const instructionInputs = document.querySelectorAll(".instruction-input")
    const hasInstructions = Array.from(instructionInputs).some((input) => input.value.trim() !== "")

    if (!hasInstructions) {
        showFieldError(document.getElementById("instructionsValue"), "Phải có ít nhất một bước thực hiện")
        isValid = false
    }

    return isValid
}

// Show field error
function showFieldError(field, message) {
    const errorElement = field.parentNode.querySelector(".validation-message")
    if (errorElement) {
        errorElement.textContent = message
    }
    field.style.borderColor = "#dc2626"
}

// Simple message functions
function showSuccessMessage(message) {
    showMessage(message, "success")
}

function showErrorMessage(message) {
    showMessage(message, "error")
}

function showWarningMessage(message) {
    showMessage(message, "warning")
}

function showMessage(message, type) {
    // Remove existing messages
    const existing = document.querySelector(".simple-message")
    if (existing) existing.remove()

    const messageEl = document.createElement("div")
    messageEl.className = `simple-message ${type}`
    messageEl.textContent = message

    messageEl.style.cssText = `
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
            : type === "error"
                ? "linear-gradient(135deg, #dc2626 0%, #b91c1c 100%)"
                : "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)"
        };
        box-shadow: 0 8px 20px rgba(0, 0, 0, 0.2);
        transition: opacity 0.2s ease;
    `

    document.body.appendChild(messageEl)

    setTimeout(() => {
        messageEl.style.opacity = "0"
        setTimeout(() => messageEl.remove(), 200)
    }, 3000)
}

// Global functions for compatibility
window.previewImage = previewImage
window.addIngredient = addIngredient
window.addInstruction = addInstruction
window.removeIngredient = removeIngredient
window.removeInstruction = removeInstruction
window.triggerFileInput = triggerFileInput
