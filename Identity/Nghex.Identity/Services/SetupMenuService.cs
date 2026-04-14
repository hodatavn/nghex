using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Nghex.Identity.Models;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Services
{
    /// <summary>
    /// Loads setup menu from an XML file (menu.config) into MenuNodeDto tree.
    /// Used only for pre-DB setup login (setup_token=true).
    /// </summary>
    public class SetupMenuService : ISetupMenuService
    {
        private readonly IConfiguration _configuration;

        public SetupMenuService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IReadOnlyList<MenuNodeDto>> GetSetupMenuAsync(CancellationToken cancellationToken = default)
        {
            // Support both root keys and nested under SetupSettings
            var fileName =
                _configuration["SetupSettings:MenuConfig:FileName"]
                ?? _configuration["MenuConfig:FileName"]
                ?? "menu.config";

            var relativePath =
                _configuration["SetupSettings:MenuConfig:RelativePath"]
                ?? _configuration["MenuConfig:RelativePath"]
                ?? Path.Combine("data", "setup", fileName);

            try
            {
                var xml = await ReadConfigFileAsync(relativePath, cancellationToken);
                if (string.IsNullOrWhiteSpace(xml))
                    return FallbackMenu();

                var doc = XDocument.Parse(xml, LoadOptions.None);

                // Root can be <Menus> or any container. We accept direct <Menu> children too.
                var root = doc.Root;
                if (root == null)
                    return FallbackMenu();

                var menuElements = root.Name.LocalName.Equals("Menu", StringComparison.OrdinalIgnoreCase)
                    ? new[] { root }
                    : root.Elements().Where(e => e.Name.LocalName.Equals("Menu", StringComparison.OrdinalIgnoreCase));

                var result = menuElements
                    .Select(e => ParseMenuElement(e, parentKey: null))
                    .Where(n => n != null)
                    .Cast<MenuNodeDto>()
                    .OrderBy(n => n.SortOrder)
                    .ThenBy(n => n.Title)
                    .ToList();

                return result.Count > 0 ? result : FallbackMenu();
            }
            catch
            {
                return FallbackMenu();
            }
        }

        private static MenuNodeDto? ParseMenuElement(XElement el, string? parentKey)
        {
            var key = (string?)el.Attribute("key") ?? (string?)el.Attribute("Key");
            var title = (string?)el.Attribute("title") ?? (string?)el.Attribute("Title");
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(title))
                return null;

            var route = (string?)el.Attribute("route") ?? (string?)el.Attribute("Route");
            var icon = (string?)el.Attribute("icon") ?? (string?)el.Attribute("Icon");
            var sortOrder = TryInt((string?)el.Attribute("sortOrder") ?? (string?)el.Attribute("SortOrder")) ?? 0;
            var isAccessible = TryBool((string?)el.Attribute("isAccessible") ?? (string?)el.Attribute("IsAccessible")) ?? true;
            var pluginName = (string?)el.Attribute("pluginName") ?? (string?)el.Attribute("PluginName");
            var isActive = TryBool((string?)el.Attribute("isActive") ?? (string?)el.Attribute("IsActive")) ?? true;
            var node = new MenuNodeDto
            {
                MenuKey = key,
                ParentKey = parentKey,
                Title = title,
                Route = string.IsNullOrWhiteSpace(route) ? null : route,
                Icon = string.IsNullOrWhiteSpace(icon) ? null : icon,
                PluginName = pluginName,
                SortOrder = sortOrder,
                IsActive = isActive,
                IsAccessible = isAccessible,
                Children = new List<MenuNodeDto>()
            };

            var children = el.Elements().Where(e => e.Name.LocalName.Equals("Menu", StringComparison.OrdinalIgnoreCase));
            foreach (var child in children)
            {
                var childNode = ParseMenuElement(child, parentKey: key);
                if (childNode != null)
                    node.Children.Add(childNode);
            }

            node.Children = node.Children
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Title)
                .ToList();

            return node;
        }

        private static int? TryInt(string? raw) => int.TryParse(raw, out var n) ? n : null;
        private static bool? TryBool(string? raw) => bool.TryParse(raw, out var b) ? b : null;

        private static IReadOnlyList<MenuNodeDto> FallbackMenu()
        {
            // Safe fallback: minimal setup menu
            return new List<MenuNodeDto>
            {
                new MenuNodeDto
                {
                    MenuKey = "setup",
                    ParentKey = null,
                    Title = "Setup",
                    Route = null,
                    Icon = "settings",
                    PluginName = null,
                    SortOrder = 10,
                    IsAccessible = true,
                    IsActive = true,
                    Children = new List<MenuNodeDto>
                    {
                        new MenuNodeDto
                        {
                            MenuKey = "setup.database",
                            ParentKey = "setup",
                            Title = "Setup Database",
                            Route = "/setup/database",
                            Icon = "storage",
                            PluginName = null,
                            SortOrder = 10,
                            IsAccessible = true,
                            IsActive = true
                        }
                    }
                }
            };
        }

        private static async Task<string> ReadConfigFileAsync(string relativePath, CancellationToken cancellationToken)
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(exeDir, "..", "..", ".."));
            var candidates = new List<string>
            {
                Path.Combine(projectRoot, relativePath),
                Path.Combine(Directory.GetCurrentDirectory(), relativePath),
                Path.Combine(exeDir, relativePath)
            };

            var fullPath = candidates.FirstOrDefault(File.Exists);
            if (string.IsNullOrWhiteSpace(fullPath))
                return string.Empty;

            return await File.ReadAllTextAsync(fullPath, cancellationToken);
        }
    }
}


