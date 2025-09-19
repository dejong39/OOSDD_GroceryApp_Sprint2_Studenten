using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
// using Kotlin.Internal;
using System.Collections.ObjectModel;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            //Maak de lijst AvailableProducts leeg
            AvailableProducts = [];

            //Haal de lijst met producten op
            List<Product> producten = _productService.GetAll();

            //Controleer of het product al op de boodschappenlijst staat, zo niet zet het in de AvailableProducts lijst
            foreach (var product in producten)
            {
                bool bestaatAl = false;
                foreach (var item in MyGroceryListItems)
                {
                    if (item.ProductId == product.Id) bestaatAl = true;
                }
                //Houdt rekening met de voorraad (als die nul is kun je het niet meer aanbieden).
                if (!bestaatAl && product.Stock > 0) AvailableProducts.Add(product); 
            }         
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)
        {
            //Controleer of het product bestaat en dat de Id > 0
            if (product == null) return;

            //Maak een GroceryListItem met Id 0 en vul de juiste productid en grocerylistid
            GroceryListItem newItem = new(0, GroceryList.Id, product.Id, 1);

            //Voeg het GroceryListItem toe aan de dataset middels de _groceryListItemsService
            _groceryListItemsService.Add(newItem);

            //Werk de voorraad (Stock) van het product bij en zorg dat deze wordt vastgelegd (middels _productService)
            Product updatedProduct = new(product.Id, product.Name, product.Stock - 1);

            //Werk de lijst AvailableProducts bij, want dit product is niet meer beschikbaar
            AvailableProducts.Remove(product);

            //call OnGroceryListChanged(GroceryList);
            OnGroceryListChanged(GroceryList);
        }
    }
}
