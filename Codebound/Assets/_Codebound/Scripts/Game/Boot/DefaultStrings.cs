using System.Collections.Generic;
using Codebound.Core;

namespace Codebound.Game
{
    /// <summary>
    /// Built-in string tables for the languages done so far (pt-PT, en). Will move to data files
    /// (Unity Localization or JSON) in Etapa 8; keys stay the same.
    /// </summary>
    public static class DefaultStrings
    {
        private static bool _registered;

        public static void RegisterAll()
        {
            if (_registered) return;
            _registered = true;
            Loc.Register("pt-PT", PtPt);
            Loc.Register("en", En);
            Loc.SetLanguage("pt-PT");
        }

        private static readonly Dictionary<string, string> PtPt = new Dictionary<string, string>
        {
            { "name.item.scrap", "Sucata" },
            { "name.item.stone", "Pedra" },
            { "name.item.wood", "Madeira" },
            { "name.item.iron_ore", "Minério de ferro" },
            { "name.item.copper_ore", "Minério de cobre" },
            { "name.item.coal", "Carvão" },
            { "name.item.silicon", "Silício" },
            { "name.item.titanium_ore", "Minério de titânio" },
            { "name.item.uranium_ore", "Minério de urânio" },
            { "name.item.oil", "Petróleo" },
            { "name.item.alien_crystal", "Cristal alienígena" },
            { "name.structure.wall", "Parede" },
            { "name.structure.floor", "Chão" },
            { "name.structure.crate", "Caixa de armazenamento" },
            { "name.structure.shelter_module", "Módulo de abrigo" },
            { "hud.inventory", "Inventário" },
            { "hud.build", "Construção" },
            { "hud.build_mode_on", "Modo de construção" },
            { "hud.storage", "Armazenamento" },
            { "hud.close", "Fechar" },
            { "hud.cost", "Custo" },
            { "hud.dev_notice", "HUD de desenvolvimento (temporário) — o HUD final será programável em HTML/CSS" },
            { "hud.controls", "WASD mover · E/clique recolher · B construir · R rodar · 1-4 escolher · botão direito desmontar · roda zoom · Tab inventário · F1 idioma" },
            { "hud.target.harvest", "Recolher {0} ({1} restantes)" },
            { "hud.target.storage", "Abrir {0}" },
            { "toast.gained", "+{0} {1}" },
            { "toast.inventory_full", "Inventário cheio" },
            { "toast.placed", "Construído: {0}" },
            { "toast.removed", "Desmontado: {0}" },
            { "toast.blocked", "Não é possível construir aqui" },
            { "toast.occupied", "Já existe uma estrutura aqui" },
            { "toast.no_resources", "Recursos insuficientes" },
            { "toast.no_space_refund", "Sem espaço no inventário para recuperar os materiais" },
            { "toast.language", "Idioma: {0}" },
        };

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { "name.item.scrap", "Scrap" },
            { "name.item.stone", "Stone" },
            { "name.item.wood", "Wood" },
            { "name.item.iron_ore", "Iron ore" },
            { "name.item.copper_ore", "Copper ore" },
            { "name.item.coal", "Coal" },
            { "name.item.silicon", "Silicon" },
            { "name.item.titanium_ore", "Titanium ore" },
            { "name.item.uranium_ore", "Uranium ore" },
            { "name.item.oil", "Oil" },
            { "name.item.alien_crystal", "Alien crystal" },
            { "name.structure.wall", "Wall" },
            { "name.structure.floor", "Floor" },
            { "name.structure.crate", "Storage crate" },
            { "name.structure.shelter_module", "Shelter module" },
            { "hud.inventory", "Inventory" },
            { "hud.build", "Build" },
            { "hud.build_mode_on", "Build mode" },
            { "hud.storage", "Storage" },
            { "hud.close", "Close" },
            { "hud.cost", "Cost" },
            { "hud.dev_notice", "Development HUD (temporary) — the final HUD will be programmable in HTML/CSS" },
            { "hud.controls", "WASD move · E/click harvest · B build · R rotate · 1-4 select · right click deconstruct · wheel zoom · Tab inventory · F1 language" },
            { "hud.target.harvest", "Harvest {0} ({1} left)" },
            { "hud.target.storage", "Open {0}" },
            { "toast.gained", "+{0} {1}" },
            { "toast.inventory_full", "Inventory full" },
            { "toast.placed", "Built: {0}" },
            { "toast.removed", "Deconstructed: {0}" },
            { "toast.blocked", "Cannot build here" },
            { "toast.occupied", "A structure is already here" },
            { "toast.no_resources", "Not enough resources" },
            { "toast.no_space_refund", "No inventory space to recover the materials" },
            { "toast.language", "Language: {0}" },
        };
    }
}
