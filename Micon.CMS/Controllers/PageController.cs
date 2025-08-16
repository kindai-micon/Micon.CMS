using Micon.CMS.Models;
using Micon.CMS.Library.Models.Form;
using Micon.CMS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Micon.CMS.Controllers
{
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPageTemplateRepository _pageTemplateRepository;

        public PageController(ApplicationDbContext context, IPageTemplateRepository pageTemplateRepository)
        {
            _context = context;
            _pageTemplateRepository = pageTemplateRepository;
        }

        public async Task<IActionResult> Index(Guid categoryId, Guid pageId)
        {
            var page = await _context.Pages
                .Include(p => p.PageTemplate)
                .FirstOrDefaultAsync(p => p.PageTemplate.PageCategoryId == categoryId && p.Id == pageId);

            if (page == null || page.PageTemplate == null)
            {
                return NotFound();
            }

            var hierarchy = await _pageTemplateRepository.GetComponentHierarchy(page.PageTemplate, HttpContext.RequestAborted);

            // Get all component settings for the current page
            var componentIds = hierarchy.Select(h => h.ChildComponentId).ToList();
            var settings = await _context.ComponentSettings
                                         .Where(s => s.PageId == page.Id && componentIds.Contains(s.ComponentId))
                                         .ToListAsync();

            var viewModel = BuildViewModelTree(hierarchy, settings);

            return View(viewModel);
        }
        [HttpGet(nameof(Test))]
        public IActionResult Test()
        {
            PageComponentViewModel viewModel = new PageComponentViewModel();
            
            var rootNode = new PageComponentViewModel() { ComponentName = "ClassLibrary1.Components.C1ViewComponent", SlotName = "Main" ,PackageId = new Guid("fd90ba15-f824-456a-b451-7b0bd102c273") };
            var child = new PageComponentViewModel() { ComponentName = "ClassLibrary1.Components.C2ViewComponent", SlotName = "ChildSlot" ,PackageId = new Guid("fd90ba15-f824-456a-b451-7b0bd102c273") };
            rootNode.Children.Add(child);
            viewModel.Children.Add(rootNode);
            return View("index",viewModel);
        }
        private PageComponentViewModel BuildViewModelTree(List<ComponentHierarchy> hierarchy, List<ComponentSetting> settings)
        {
            if (hierarchy == null || !hierarchy.Any())
            {
                return new PageComponentViewModel();
            }

            var settingsLookup = settings.ToLookup(s => s.ComponentId);

            var lookup = hierarchy.ToDictionary(
                h => h.ChildId,
                h => new PageComponentViewModel
                {
                    ComponentId = h.ChildComponentId,
                    ComponentName = h.ChildComponentName,
                    SlotName = h.SlotName,
                    Settings = settingsLookup[h.ChildComponentId].ToDictionary(s => s.Key, s => s.Value)
                });

            var rootNode = new PageComponentViewModel { ComponentName = "Root" ,SlotName = "Main" };

            foreach (var item in hierarchy)
            {
                if (item.ParentId.HasValue && lookup.ContainsKey(item.ParentId.Value))
                {
                    lookup[item.ParentId.Value].Children.Add(lookup[item.ChildId]);
                }
                else
                {
                    rootNode.Children.Add(lookup[item.ChildId]);
                }
            }
            
            return rootNode.Children.Count == 1 ? rootNode.Children.First() : rootNode;
        }
    }
}

