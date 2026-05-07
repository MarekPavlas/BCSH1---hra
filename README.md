# BCSH1 - hra

Semestrální projekt vytvořený v Unity. Jedná se o 3D survival / wave-based hru, ve které hráč přežívá vlny nepřátel, sbírá peníze, nakupuje zbraně a pasivní itemy ve shopu mezi vlnami.

## Popis hry

Hráč se pohybuje po vybrané mapě a snaží se přežít vlny nepřátel. Nepřátelé se spawnují během vln, pohybují se pomocí NavMesh a útočí na hráče zblízka nebo na dálku. Po zabití mohou nepřátelé dropnout peníze nebo healing item. Mezi vlnami se otevře obchod, kde si hráč může koupit zbraně, upgrady nebo pasivní itemy.

## Hlavní prvky hry

- pohyb hráče ve 3D prostředí
- kamera třetí osoby
- nepřátelé pohybující se pomocí NavMesh
- melee a ranged nepřátelé
- wave systém
- shop mezi vlnami
- zbraně a upgrady
- pasivní itemy ovlivňující statistiky hráče
- sbírání peněz a healing pickupů
- ukládání statistik pomocí PlayerPrefs
- hlavní menu, výběr mapy, pause menu, win/death obrazovky

## Ukázky ze hry

| # | Název | Obrázek | Popis |
|---:|---|---|---|
| 1 | Výběr mapy | <img src="Screenshots/map-select.png" width="350"> | Obrazovka pro výběr jedné ze tří map: Forest, Mars a Mountain. |
| 2 | Lesní mapa | <img src="Screenshots/les-map.png" width="350"> | Ukázka prostředí lesní mapy. |
| 3 | Mars mapa | <img src="Screenshots/mars-map.png" width="350"> | Ukázka prostředí Mars mapy. |
| 4 | Horská mapa | <img src="Screenshots/hora-map.png" width="350"> | Ukázka prostředí horské mapy. |
| 5 | Průběh hry | <img src="Screenshots/midgame.png" width="350"> | Gameplay během vlny nepřátel. V horní části je HUD s HP, aktuální vlnou a goldy. |
| 6 | Shop mezi vlnami | <img src="Screenshots/shop.png" width="350"> | Obchod mezi vlnami, kde hráč nakupuje zbraně, upgrady a pasivní itemy. |

# Použité zdroje

## 1. Grafické assety a ikony

| # | Typ | Název | Zdroj/Link | Poznámka |
|---:|---|---|---|---|
| 1 | Grafika (Items) | Free RPG Halloween Icons | [CraftPix](https://craftpix.net/freebies/free-rpg-halloween-icons/?num=1&count=675&sq=item%20icons&pos=4) | Ikony pasivních itemů a předmětů do herního shopu |
| 2 | Grafika (Weapons) | Free Cyberpunk Weapon Icons | [CraftPix](https://craftpix.net/freebies/free-cyberpunk-weapon-icons-50-png-512x512/) | Ikony zbraní použité v UI shopu a inventáře |
| 3 | Grafika (Items) | Free RPG Hunting Game Icons | [itch.io](https://free-game-assets.itch.io/free-rpg-hunting-game-512512-icons) | Ikony pasivních itemů a předmětů do herního shopu |
| 4 | Grafika (Items) | Free RPG Wing Game Icons | [itch.io](https://free-game-assets.itch.io/free-rpg-wing-game-icons) | Ikony pasivních itemů a předmětů do herního shopu |
| 5 | Grafika (Items) | Free Cyberpunk Radiation Game Icons | [itch.io](https://free-game-assets.itch.io/free-cyberpunk-radiation-game-icons) | Ikony záření a jedů pro pasivní itemy |
| 6 | Grafika (Items) | Free Cyberpunk Medicine Icons | [itch.io](https://free-game-assets.itch.io/free-cyberpunk-medicine-icons) | Ikony léčebných a podpůrných itemů |
| 7 | Grafika (Items) | 50 Free RPG Mushroom Icons | [itch.io](https://free-game-assets.itch.io/50-free-rpg-mushroom-icons) | Ikony přírodních a alchymistických itemů |
| 8 | Grafika (Items) | Free RPG Dragon Loot Icons | [itch.io](https://free-game-assets.itch.io/free-rpg-dragon-loot-icons) | Ikony kořisti a vzácných předmětů do herního shopu |

## 2. 3D modely a prostředí

| # | Typ | Název | Zdroj/Link | Poznámka |
|---:|---|---|---|---|
| 1 | 3D model (Pickup) | 3D Cartoon Cute Safe Pack | [Unity Asset Store](https://assetstore.unity.com/packages/3d/props/3d-cartoon-cute-safe-pack-297716) | 3D model gemu / peněz |
| 2 | 3D model (Player) | Free Low Poly Human RPG Character | [Unity Asset Store](https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/free-low-poly-human-rpg-character-219979) | 3D model hráčské postavy |
| 3 | 3D model (Enemy) | RPG Monster Buddy PBR Polyart | [Unity Asset Store](https://assetstore.unity.com/packages/3d/characters/creatures/rpg-monster-buddy-pbr-polyart-253961) | 3D model nepřítele |
| 4 | 3D model (Enemy) | RPG Monster Duo PBR Polyart | [Unity Asset Store](https://assetstore.unity.com/packages/3d/characters/creatures/rpg-monster-duo-pbr-polyart-157762) | 3D model nepřítele |
| 5 | Prostředí | Natural Environment Mobile | [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/landscapes/natural-environment-mobile-324098) | Prostředí pro lesní mapu |
| 6 | Grafika (UI) | Buttons Set | [Unity Asset Store](https://assetstore.unity.com/packages/2d/gui/buttons-set-211824) | Grafické prvky tlačítek pro herní UI |
| 7 | 3D model (Pickup) | Free Healing Item Including C# Script | [Unity Asset Store](https://assetstore.unity.com/packages/3d/props/free-healing-item-including-c-script-275780) | Model healing itemu |
| 8 | 3D model (Weapons) | Weapons Pack Bullets | [Unity Asset Store](https://assetstore.unity.com/packages/3d/props/weapons/weapons-pack-bullets-302702) | 3D modely zbraní a projektilů |
| 9 | Prostředí | Mars Landscape 3D | [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/landscapes/mars-landscape-3d-175814) | Prostředí pro Mars mapu |
| 10 | Prostředí | Rocks and Terrains Pack Low Poly | [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/rocks-and-terrains-pack-low-poly-281733) | Low-poly skály a terénní objekty pro Mars i lesní mapu |

## 3. Unity dokumentace

| # | Typ | Název | Zdroj/Link | Poznámka |
|---:|---|---|---|---|
| 1 | Dokumentace | SceneManager.LoadScene | [Unity Docs](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadScene.html) | Načítání scén, výběr mapy, restart hry a návrat do hlavního menu |
| 2 | Dokumentace | PlayerPrefs | [Unity Docs](https://docs.unity3d.com/ScriptReference/PlayerPrefs.html) | Ukládání statistik hráče, například úmrtí, výher, zabitých nepřátel a získaných peněz |
| 3 | Dokumentace | ScriptableObject | [Unity Docs](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html) | Datové assety pro zbraně, pasivní itemy, upgrady a scaling vln |
| 4 | Dokumentace | Button.onClick | [Unity Docs](https://docs.unity3d.com/2018.3/Documentation/ScriptReference/UI.Button-onClick.html) | Funkce tlačítek v menu, shopu, pause menu a win/death obrazovkách |
| 5 | Dokumentace | NavMeshAgent.SetDestination | [Unity Docs](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.SetDestination.html) | Pohyb nepřátel směrem k hráči pomocí NavMesh |
| 6 | Dokumentace | NavMesh.SamplePosition | [Unity Docs](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/AI.NavMesh.SamplePosition.html) | Kontrola a hledání platné pozice na NavMeshi při spawnování nepřátel |
| 7 | Dokumentace | CharacterController.Move | [Unity Docs](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/CharacterController.Move.html) | Pohyb hráče po mapě |
| 8 | Dokumentace | Collider.OnTriggerEnter | [Unity Docs](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Collider.OnTriggerEnter.html) | Zásahy projektilů a detekce sebrání pickupů |
| 9 | Dokumentace | Physics.OverlapSphere | [Unity Docs](https://docs.unity3d.com/ScriptReference/Physics.OverlapSphere.html) | Detekce objektů v okolí, například melee útoky nepřátel nebo magnet na pickupy |
| 10 | Dokumentace | Physics.Raycast | [Unity Docs](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Physics.Raycast.html) | Kontrola line of sight a pokládání dropů na zem |
| 11 | Dokumentace | Time.timeScale | [Unity Docs](https://docs.unity3d.com/ScriptReference/Time-timeScale.html) | Pozastavení hry při pause menu a shopu |
| 12 | Dokumentace | Animator.SetBool | [Unity Docs](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Animator.SetBool.html) | Přepínání animačních stavů, například běh nebo smrt |
| 13 | Dokumentace | Animator.SetTrigger | [Unity Docs](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Animator.SetTrigger.html) | Spouštění jednorázových animací, například útok nebo zásah |
| 14 | Dokumentace | Vector3.MoveTowards | [Unity Docs](https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html) | Plynulé přitahování peněz a gemů směrem k hráči |

## 4. YouTube tutoriály

| # | Typ | Název | Zdroj/Link | Poznámka |
|---:|---|---|---|---|
| 1 | Tutorial | Animator Controller | [YouTube](https://www.youtube.com/watch?v=oyJN53HjExg) | Animace postav a práce s Animator Controllerem |
| 2 | Tutorial | Passive Items | [YouTube](https://www.youtube.com/watch?v=iU6mKyQjOYI) | Návrh pasivních itemů a jejich efektů |
| 3 | Tutorial | Player Stats | [YouTube](https://www.youtube.com/watch?v=AheSE2wlavk) | Systém hráčových statů a bonusů |
| 4 | Tutorial | Level Select | [YouTube](https://www.youtube.com/watch?v=2XQsKNHk1vk) | Výběr mapy v hlavním menu |
| 5 | Tutorial | Canvas UI | [YouTube](https://www.youtube.com/watch?v=1OwQflHq5kg) | Nastavení Canvasu a škálování UI |
| 6 | Tutorial | PlayerPrefs | [YouTube](https://www.youtube.com/watch?v=x-5lrUSBwXY) | Ukládání dat pomocí PlayerPrefs |
| 7 | Tutorial | Stat Upgrades | [YouTube](https://www.youtube.com/watch?v=HuI3vcMIggM) | Úprava statů pomocí upgradů |
| 8 | Tutorial | Shop System | [YouTube](https://www.youtube.com/watch?v=MSYEXW0cOLU) | Obchod, nákup položek a práce s cenou |
| 9 | Tutorial | Health Pickup | [YouTube](https://www.youtube.com/watch?v=szYMDBFUtVs) | Healing pickup a doplnění HP |
| 10 | Tutorial | Enemy Drops | [YouTube](https://www.youtube.com/watch?v=yjZ5mLNll5M) | Drop peněz z nepřátel |
| 11 | Tutorial | Health and Damage | [YouTube](https://www.youtube.com/watch?v=upvgX2D7wEg) | Health a damage systém hráče a nepřátel |
| 12 | Tutorial | Survival Gameplay Loop | [YouTube](https://www.youtube.com/watch?v=Nxg0vQk05os) | Základní survival gameplay loop |
| 13 | Tutorial | Cinemachine Camera | [YouTube](https://www.youtube.com/watch?v=P_ibDJhFVMU) | Kamera třetí osoby pomocí Cinemachine |
| 14 | Tutorial | Save / Load System | [YouTube](https://www.youtube.com/watch?v=XOjd_qU2Ido) | Ukládání a načítání statistik |
| 15 | Tutorial | Pickup System | [YouTube](https://www.youtube.com/watch?v=EfUCEwKmcjc) | Sbírání pickupů pomocí trigger colliderů |
| 16 | Tutorial | Scene Loading | [YouTube](https://www.youtube.com/watch?v=3I5d2rUJ0pE) | Přepínání scén a načítání map |
| 17 | Tutorial | Pause Menu | [YouTube](https://www.youtube.com/watch?v=JivuXdrIHK0) | Pause menu, Resume, Restart a Quit |
| 18 | Tutorial | ScriptableObject | [YouTube](https://www.youtube.com/watch?v=7jxS8HIny3Q) | Použití ScriptableObject assetů |
| 19 | Tutorial | ScriptableObject Shop | [YouTube](https://www.youtube.com/watch?v=kUwnfkYcaFU) | Shop založený na ScriptableObject datech |
| 20 | Tutorial | Enemy Spawning | [YouTube](https://www.youtube.com/watch?v=hI7zH3OE8Y8) | Spawnování nepřátel |
| 21 | Tutorial | NavMesh Spawning | [YouTube](https://www.youtube.com/watch?v=5uO0dXYbL-s) | Spawnování objektů na NavMesh |
| 22 | Tutorial | Shooting System | [YouTube](https://www.youtube.com/watch?v=om-SS-CBZ8g) | Základ střelby a projektilů |
| 23 | Tutorial | Weapon System | [YouTube](https://www.youtube.com/watch?v=JCngTlb2R2c) | Systém zbraní a jejich nastavení |
| 24 | Tutorial | Pickup Magnet | [YouTube](https://www.youtube.com/watch?v=z_gRquF8SGs) | Přitahování gemů k hráči |
| 25 | Tutorial | Ranged Enemy | [YouTube](https://www.youtube.com/watch?v=QzitQSLhfG0) | Ranged enemy a střelba na hráče |
| 26 | Tutorial | NavMesh Enemy AI | [YouTube](https://www.youtube.com/watch?v=KZROVLPQdWc) | Pohyb nepřátel za hráčem pomocí NavMesh |

## 5. Generativní AI

| # | Typ | Název | Zdroj/Link | Poznámka |
|---:|---|---|---|---|
| 1 | Generativní AI | ChatGPT | [OpenAI ChatGPT](https://chatgpt.com/) | Pomocný nástroj pro konzultaci kódu, ladění chyb, návrh architektury a generování boilerplate kódu |
