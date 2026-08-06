using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

/// <summary>Modal popup used for both "Add Category" and "Edit Category".</summary>
public class CategoryEditForm : BaseForm
{
    private readonly Category _existingCategory;
    private readonly TextBox _nameBox;
    private readonly TextBox _descriptionBox;

    public bool WasSaved { get; private set; }

    public CategoryEditForm(Category existingCategory = null)
    {
        _existingCategory = existingCategory;

        Text = existingCategory == null ? "Add Category" : "Edit Category";
        ClientSize = new Size(420, 275);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;

        var nameLabel = UiHelpers.MakeLabel("Category Name", 30, 30, 150);
        _nameBox = UiHelpers.MakeTextBox(30, 55, 340);

        var descriptionLabel = UiHelpers.MakeLabel("Description", 30, 100, 150);
        _descriptionBox = UiHelpers.MakeTextBox(30, 125, 340, 60);
        _descriptionBox.Multiline = true;

        var saveButton = UiHelpers.MakeButton("Save", 130, 205, 120);
        saveButton.Click += async (_, _) => await SaveAsync();

        var cancelButton = UiHelpers.MakeButton("Cancel", 260, 205, 110);
        cancelButton.Click += (_, _) => Close();

        Controls.Add(nameLabel);
        Controls.Add(_nameBox);
        Controls.Add(descriptionLabel);
        Controls.Add(_descriptionBox);
        Controls.Add(saveButton);
        Controls.Add(cancelButton);

        if (existingCategory != null)
        {
            _nameBox.Text = existingCategory.CategoryName;
            _descriptionBox.Text = existingCategory.Description;
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_nameBox.Text))
        {
            AppMessageBox.Show("Category name is required.", "Missing information");
            return;
        }

        try
        {
            if (_existingCategory == null)
            {
                await CategoryService.CreateCategoryAsync(_nameBox.Text.Trim(), _descriptionBox.Text.Trim());
            }
            else
            {
                await CategoryService.UpdateCategoryAsync(_existingCategory.CategoryId, _nameBox.Text.Trim(), _descriptionBox.Text.Trim());
            }
            WasSaved = true;
            Close();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not save category");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }
}
