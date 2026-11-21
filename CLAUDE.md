# Regole di progetto: gioco 3D lowpoly dark (stile Inside) in Unity

Queste regole valgono SEMPRE. Rispondi in italiano. Sii breve.

## 1. Il gioco
- 3D lowpoly, stile *Inside*: forme semplici, silhouette leggibili, colori piatti e lineari (niente texture complesse).
- Atmosfera dark: palette desaturata, pochi colori d'accento, nebbia/fog, contrasto luce-ombra.
- Target: **minimo 60 FPS** (anche su hardware medio). Ogni soluzione deve essere performante e semplice.

## 2. Flusso di lavoro (obbligatorio)
1. **Controlla la connessione MCP** a Unity e Blender (chiamata leggera) prima di agire. Se non risponde, fermati e avvisami: non tentare workaround.
2. **Spiega la soluzione** in modo semplice, come a una paperella di gomma: cosa vuoi fare, perché, come funziona. Poche righe, zero gergo inutile.
3. **Aspetta il mio feedback esplicito** ("ok", "procedi", ecc.). Senza feedback NON modificare codice, scene, asset o file.
4. Implementa solo ciò che è stato approvato, niente extra non richiesti.
5. Se cambio idea, torna al punto 2.

## 3. Semplicità prima di tutto
- Vietato over-ingegnerizzare: no pattern, interfacce, layer o sistemi generici "per il futuro" se non servono ora.
- Preferisci la soluzione più semplice che funziona e che si legge in 30 secondi.
- Se ci sono 2 opzioni, proponi la più semplice e cita l'altra in una riga.
- Non refactorare codice che non c'entra con la richiesta.

## 4. Performance Unity (60 FPS)
- Zero allocazioni in `Update`/`FixedUpdate`: no `new`, no LINQ, no concatenazione stringhe, no `foreach` su collezioni che allocano.
- Metti in cache `GetComponent`, `Camera.main`, `Transform`. Mai `Find*` o `GetComponent` in `Update`.
- Preferisci eventi/coroutine/timer a controlli ogni frame. Disattiva script che non servono.
- Object pooling per oggetti spawnati spesso (no `Instantiate`/`Destroy` ripetuti).
- Fisica: collider semplici (box/sphere/capsule), no MeshCollider non convessi, usa la Layer Collision Matrix.
- Grafica: URP, pochi materiali condivisi (colori piatti), SRP Batcher/GPU instancing, static batching dove possibile.
- Luci: preferisci baked; poche luci realtime, ombre solo dove servono. Post-processing leggero.
- Mesh: poligoni al minimo, niente dettaglio invisibile. Fog e vignetting danno atmosfera a basso costo.
- Se una soluzione può pesare, dillo e indica come misurarla (Profiler).

## 5. Unity 6 (6000.x)
- Usa solo API di Unity 6. Non inventare: se non sei sicuro di un'API, dillo.
- Obsoleti da evitare: `FindObjectOfType` -> `FindFirstObjectByType`; `Rigidbody.velocity` -> `linearVelocity`; `drag` -> `linearDamping`.
- Input: nuovo **Input System** (non `Input.GetKey`).
- Render pipeline: **URP** (se uso altro, ti avviso). Sfrutta SRP Batcher, GPU Resident Drawer e GPU occlusion culling se aiutano; attivali solo dopo averli spiegati.
- Per attese/asincrono semplice usa coroutine o `Awaitable`, non librerie esterne.
- Dopo modifiche agli script controlla la **Console** di Unity (errori/warning) prima di dire "fatto".

## 6. Codice
- C# semplice, nomi chiari, classi piccole con una sola responsabilità reale.
- `[SerializeField] private` invece di campi pubblici. Commenti solo dove il "perché" non è ovvio.
- Un MonoBehaviour per file, nome file = nome classe.
- Niente dipendenze o package nuovi senza chiedermelo.

## 7. Risparmio token
- Risposte corte: niente preamboli, niente riassunti finali ripetitivi, niente codice già noto riscritto per intero (mostra solo le modifiche).
- Non rileggere file o scene già visti in questa conversazione, salvo modifiche.
- Leggi solo ciò che serve (singolo file/oggetto, non l'intera gerarchia).
- **Screenshot: mai**, a meno che servano a te per ragionare e non ci siano alternative testuali.
- Operazione ripetibile? Crea uno **script Python/Editor script C#/markdown** in `Tools/` (o `Docs/`) e riusalo invece di rifare il lavoro. Prima di creare uno script controlla se esiste già. Annota ogni script in `Tools/README.md` (1 riga: nome + a cosa serve).
- Decisioni utili al futuro (convenzioni, scelte prese) vanno in `Docs/DECISIONI.md`, 1-2 righe ciascuna.

## 8. Blender (non lo conosco: fai tu tutto)
- Io non so usare Blender. Fai tu ogni operazione via MCP/script, senza chiedermi di cliccare o usare menu. Se un passaggio manuale è davvero inevitabile, dammi istruzioni numerate, brevi, con il nome esatto di menu/pulsanti.
- Spiega cosa fai con parole semplici (es. "unisco questi vertici per ridurre i poligoni"), senza termini Blender non spiegati.
- Stile: lowpoly, flat shading, un colore = un materiale semplice (niente texture se evitabile), palette coerente col gioco.
- Budget poligoni: tienili al minimo; oggetti di scena piccoli idealmente sotto qualche centinaio di tris, personaggio sotto qualche migliaio. Dimmi se superi.
- Export per Unity: 1 unità = 1 metro, applica scala/rotazione, pivot sensato (base dell'oggetto), nomi in PascalCase, **FBX** pulito (niente camere/luci inutili).
- Crea e riusa script Python in `Tools/Blender/` (es. export, applica materiali della palette, genera forme base). Controlla `Tools/README.md` prima di rifare qualcosa.
- Mantieni una **palette centrale** (`Docs/PALETTE.md`: nome colore + hex) usata sia in Blender che in Unity, così gli asset restano coerenti.
- Prima di creare un asset nuovo, proponi la forma in 2-3 righe e aspetta il mio ok (regola feedback valida anche qui).

## 9. Organizzazione progetto
- Cartelle: `Scripts/`, `Prefabs/`, `Materials/`, `Models/`, `Scenes/`, `Tools/` (con `Tools/Blender/`), `Docs/`.
- Prefab per ogni oggetto riutilizzabile. Nomi in inglese, PascalCase.
- Suggerisci un commit Git a ogni feature funzionante (non committare tu senza chiedere).

## 10. Quando sei incerto
- Fai UNA domanda mirata invece di indovinare. Non inventare API Unity: se non sei sicuro, dillo.
