using Frontend.Models;
using Frontend.Services;
using Frontend.Utilities;

namespace Frontend.Forms.Admin;

public class AdminCategoriesForm : BaseForm
{
    private readonly DataGridView _grid;
    private List<Category> _categories = new();

    public AdminCategoriesForm()
    {
        FormBorderStyle = FormBorderStyle.None;

        var heading = UiHelpers.MakeHeading("Categories", 20, 15, 300);

        var addButton = UiHelpers.MakeButton("Add Category", 780, 15, 150);
        addButton.Click += async (_, _) => await AddCategoryAsync();

        var editButton = UiHelpers.MakeButton("Edit Selected", 780, 60, 150);
        editButton.Click += async (_, _) => await EditSelectedAsync();

        var deleteButton = UiHelpers.MakeButton("Delete Selected", 780, 105, 150);
        deleteButton.Click += async (_, _) => await DeleteSelectedAsync();

        _grid = UiHelpers.MakeGrid(20, 60, 740, 500);
        _grid.Columns.Add("category_name", "Category Name");
        _grid.Columns.Add("description", "Description");
        _grid.Columns.Add("product_count", "Products");

        Controls.Add(heading);
        Controls.Add(addButton);
        Controls.Add(editButton);
        Controls.Add(deleteButton);
        Controls.Add(_grid);

        Load += async (_, _) => await LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            _categories = await CategoryService.GetCategoriesAsync();
            _grid.Rows.Clear();
            foreach (var category in _categories)
            {
                _grid.Rows.Add(category.CategoryName, category.Description, category.ProductCount);
            }
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not load categories");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }

    private Category GetSelectedCategory()
    {
        if (_grid.SelectedRows.Count == 0) return null;
        var index = _grid.SelectedRows[0].Index;
        return index >= 0 && index < _categories.Count ? _categories[index] : null;
    }

    private async Task AddCategoryAsync()
    {
        using var form = new CategoryEditForm();
        form.ShowDialog();
        if (form.WasSaved) await LoadCategoriesAsync();
    }

    private async Task EditSelectedAsync()
    {
        var category = GetSelectedCategory();
        if (category == null)
        {
            AppMessageBox.Show("Please select a category first.", "No selection");
            return;
        }

        using var form = new CategoryEditForm(category);
        form.ShowDialog();
        if (form.WasSaved) await LoadCategoriesAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var category = GetSelectedCategory();
        if (category == null)
        {
            AppMessageBox.Show("Please select a category first.", "No selection");
            return;
        }

        if (!AppMessageBox.Confirm($"Delete category '{category.CategoryName}'?", "Confirm Delete"))
        {
            return;
        }

        try
        {
            await CategoryService.DeleteCategoryAsync(category.CategoryId);
            await LoadCategoriesAsync();
        }
        catch (ApiException ex)
        {
            AppMessageBox.Show(ex.Message, "Could not delete category");
        }
        catch (Exception)
        {
            AppMessageBox.Show("Could not reach the server.", "Connection error");
        }
    }
}
