document.querySelectorAll("[data-password-toggle]").forEach((button) => {
  button.addEventListener("click", () => {
    const inputId = button.dataset.passwordToggle;
    const input = document.getElementById(inputId);

    if (!input) return;

    const passwordWillBeVisible = input.type === "password";
    input.type = passwordWillBeVisible ? "text" : "password";

    button.classList.toggle("is-visible", passwordWillBeVisible);
    button.setAttribute("aria-pressed", passwordWillBeVisible.toString());
    button.setAttribute(
      "aria-label",
      passwordWillBeVisible ? "Hide password" : "Show password",
    );
  });
});
