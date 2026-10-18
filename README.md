# Virtuální herbář funkcí ve VR (Virtual Herbarium of Functions in VR)

[![Unity](https://img.shields.io/badge/Unity-6%20%2F%20URP-black?style=flat&logo=unity)](https://unity.com/)
[![OpenXR](https://img.shields.io/badge/OpenXR-1.16-blue?style=flat)](https://www.khronos.org/openxr/)
[![XR Interaction Toolkit](https://img.shields.io/badge/XR%20Interaction%20Toolkit-3.3-green?style=flat)](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest)
[![C#](https://img.shields.io/badge/Language-C%23-239120?style=flat&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-orange.svg)](LICENSE)

Bakalářská práce vypracovaná na **Západočeské univerzitě v Plzni (FAV, KIV)** pod vedením **Ing. Petra Vaněčka, Ph.D.**

Cílem projektu je přenést koncept *Herbáře funkcí* (původně knižní publikace Katedry matematiky FAV KMA) do interaktivního 3D prostoru virtuální reality. Aplikace slouží jako moderní výuková pomůcka, která studentům umožňuje v reálném čase zkoumat, manipulovat a vizuálně chápat chování prostorových matematických funkcí dvou nezávislých proměnných $z = f(x, y)$.

---

## 🌟 Klíčové vlastnosti (Key Features)

- **Real-time generování 3D sítě (Mesh Generation):**
  - Dynamický výpočet vrcholů, trojúhelníků, normál a UV map přímo v reálném čase při změně předpisu či parametrů.
  - Vykreslení prostorové kartézské soustavy souřadnic (KSS) s adaptivním měřítkem.
- **Vlastní matematický parser (Math Engine):**
  - Textový předpis funkce je zpracován vlastním lexikálním analyzátorem (tokenizer) a syntaktickým analyzátorem s využitím **Shunting-Yard algoritmu** (převod do reverzní polské notace / AST).
  - Podpora pokročilých matematických funkcí, goniometrických funkcí, proměnných a dynamických parametrů.
- **Detekce a ošetření bodů nespojitosti:**
  - Automatická identifikace oblastí mimo definiční obor a nespojitostí (dělení nulou, asymptoty), zabraňující artefaktům a deformacím vykreslené sítě.
- **Plná VR interaktivita & Hand Tracking:**
  - Využití **OpenXR** a **XR Interaction Toolkit (XRI)** pro podporu moderních VR headsetů (Meta Quest, apod.).
  - Interaktivní VR Hand UI: Hand menu, změna intervalů definičního oboru, posuvníky pro reálné parametry funkcí, manipulace a rotace s grafem přímo ve virtuálním prostoru.

---

## 🛠️ Architektura a struktura projektu

```text
Assets/Scripts/
├── Core/
│   ├── DataManager.cs          # Správa datových modelů a stavu aplikace
│   ├── ErrorHandler.cs         # Zachytávání a indikace chyb v matematických výrazech
│   ├── FileWatcher.cs          # Sledování externích změn souborů se sadami funkcí
│   └── Manager.cs              # Hlavní aplikační kontrolér
├── Generator/
│   ├── Generator.cs            # Koordinátor vzorkování a tvorby grafu
│   ├── KSSGenerator.cs         # Generátor 3D os a mřížky kartézského systému
│   ├── MathPreprocess.cs       # Tokenizace, Shunting-Yard algoritmus a evaluace výrazu
│   └── MeshGenerator.cs        # Algoritmická konstrukce Unity Mesh z vypočtených bodů
├── UI/
│   ├── DesktopConsole.cs       # Pomocná konzole pro desktopový režim / debug
│   ├── HandMenuHandler.cs      # Správa kontextového menu na ruce ve VR
│   ├── DomainMenuHandler.cs    # Nastavení mezí definičního oboru (X, Y)
│   ├── ParametrMenuHandler.cs  # Nastavení konstant a parametrů předpisu
│   └── UIManager.cs            # Centrální správa Canvasů a XR UI prvků
└── VR Handling/
    ├── HandInteractionHandler.cs # Detekce gest a interakce s ovladači / rukama
    └── VRInputManager.cs         # Mapování vstupů dle OpenXR standardu
```

---

## 🚀 Požadavky a spuštění

### Požadavky:
- **Unity:** Doporučeno Unity 6 (6000.x) s podporou **Universal Render Pipeline (URP)**.
- **VR Headset:** Libovolný headset kompatibilní s rozhraním **OpenXR** (např. Meta Quest 2/3/Pro přes Link/AirLink, HTC Vive, Valve Index).

### Spuštění z editoru:
1. Naklonujte repozitář:
   ```bash
   git clone https://github.com/SrockNiper/Bachelors-project-herbarium-of-functions-in-VR.git
   ```
2. Otevřete projekt v **Unity Hubu**.
3. Otevřete výchozí scénu: `Assets/Scenes/VRScene.unity`.
4. Ujistěte se, že máte v **Project Settings -> XR Plug-in Management** aktivovaný **OpenXR**.
5. Spusťte režim Play (nebo zkompilujte build pro Windows Standalone / Android Quest).

---

## 🎓 O autorovi a práci

- **Autor:** Marek Jirušek
- **Vedoucí práce:** Ing. Petr Vaněček, Ph.D.
- **Instituce:** Západočeská univerzita v Plzni (ZČU), Fakulta aplikovaných věd (FAV), Katedra informatiky a výpočetní techniky (KIV)
- Kompletní text bakalářské práce je k dispozici v adresáři [`doc/thesis.pdf`](doc/thesis.pdf).