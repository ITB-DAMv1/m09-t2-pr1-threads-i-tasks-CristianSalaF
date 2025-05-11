[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/xs3aclQL)

## Sol·lució proposada - Exercici 1:
### Prevenció de bloquejos:

Els filòsofs parells trien primer el pal esquerre, els filòsofs senars trien primer el pal dret.
Això trenca la condició d'espera circular.

### Adminstració del cicle de vida amb IDisposable
Implementar IDisposable permet tancar els fils de treball correctament quan l'objecte ja no és necessari. 
Això garanteix que els recursos (com ara un fil de fons) s'alliberin correctament quan es crida Dispose(), 
igual que tancar els identificadors de fitxers o les connexions a la base de dades.

Exemple d'avantatge: quan l'aplicació es tanca o l'objecte es descarta, no deixarà els fils penjats en segon pla, 
cosa que evitarà fuites de memòria o processos orfes.

### Sortida acolorida:

Utilitza codis d'escapada ANSI `\u001b[XXm` en lloc de `Console.Color`.
Colors de text diferents per a cada filòsof.
Colors de fons diferents per a cada estat (pensar, esperar, menjar, etc.).
![img.png](img/img.png)

### Detecció de fam:

Controla constantment el temps des de l'últim àpat per a cada filòsof.
Finalitza la simulació si algun filòsof no ha menjat durant més de 15 segons.

### Seguiment d'estadístiques:

Seguiment del temps màxim de fam i del recompte d'àpats per a cada filòsof.
Emet estadístiques a la consola i al fitxer CSV.

## Enunciat 1 - Dades d’anàlisi
- Quants comensals han passat fam? I quin temps de mitjana?
  - Els comensals passen fam una mitjana de 2.7 segons, recopilat de 5 execucions.
- Quantes vegades ha menjat cada comensal de mitjana?
  - 15 vegades
- Record de vegades que ha menjat un mateix comensal
  - 17 vegades
- Record de menys vegades que ha menjat un comensal.
  - 13 vegades (una única vegada)

Dos de les mostres (resultats):

| Filòsof 	| Gana màxima (s) 	| Menjars totals 	|
|---------	|-----------------	|----------------	|
| 0         | 2.689             | 15             	|
| 1         | 2.948             | 15             	|
| 2         | 2.641             | 14             	|
| 3         | 2.720             | 16             	|
| 4         | 2.923             | 16             	|

| Filòsof 	| Gana màxima (s) 	| Menjars totals 	|
|---------	|-----------------	|----------------	|
| 0       	| 2.764           	| 16             	|
| 1       	| 2.788           	| 14             	|
| 2       	| 2.628           	| 14             	|
| 3       	| 2.735           	| 14             	|
| 4       	| 2.687           	| 16             	|

### Esquema
![img_1.png](img/img_1.png)

# Asteroids Game Implementation

## Solució proposada

### Descripció general
El projecte implementa un joc d'asteroides simple amb gràfics basats en consola. L'objectiu del joc és esquivar els asteroides que cauen des de la part superior de la pantalla movent el jugador (representat amb el caràcter '^') cap a l'esquerra o cap a la dreta utilitzant les tecles A i D.

### Estratègies i decisions d'implementació

#### Arquitectura del projecte
- **Program.cs**: Punt d'entrada que orquestra l'execució del joc
- **StatsManager.cs**: Gestiona l'emmagatzematge de les estadístiques del joc en un arxiu CSV

El projecte segueix un patró de disseny Model-Vista-Controlador (MVC):
- **Model**: Asteroid.cs - Representa l'estructura de dades per als asteroides
- **Controlador**: GameController.cs - Gestiona la lògica del joc, incloent moviment, col·lisions i puntuació
- **Vista**: UIController.cs - S'encarrega de la renderització del joc a la consola

#### Decisions clau d'implementació

1. **Gestió de la concurrència**:
  - S'ha utilitzat un objecte de bloqueig (`LockObject`) per sincronitzar l'accés a les dades compartides entre fils.
  - Les operacions crítiques es realitzen dins de blocs `lock` per evitar condicions de carrera.

2. **Renderització eficient**:
  - En lloc de dibuixar tota la pantalla cada vegada, només s'actualitzen les parts canviants.
  - Es manté un seguiment de les posicions anteriors per esborrar-les abans de dibuixar les noves.

3. **Disseny modular**:
  - La separació de responsabilitats entre controladors facilita el manteniment i les futures ampliacions.
  - Les constants estan definides a nivell de classe per facilitar els ajustaments i la configuració.

4. **Persistència de dades**:
  - Les estadístiques del joc (puntuació, temps jugat, vides utilitzades) es guarden en un arxiu CSV.
  - Això permet el seguiment del rendiment del jugador al llarg del temps.

5. **Simulació de Web Evaluation**:
  - S'ha implementat un sistema que simula una avaluació web en segon pla.
  - Això permet que el joc es pugui reiniciar fins que aquesta avaluació s'hagi completat.

### Estructura del projecte

```
T2_PR1_Ex2/
├── Program.cs                 # Punt d'entrada i orquestració de tasques
├── AsteroidsGame/             # Namespace principal del joc
    ├── Asteroid.cs            # Model de dades per als asteroides
    ├── GameController.cs      # Lògica del joc i gestió d'estat
    ├── UIController.cs        # Renderització i interfície d'usuari
    └── StatsManager.cs        # Emmagatzematge d'estadístiques
```

## Com has executat les tasques per tal pintar, calcular i escoltar el teclat al mateix temps?

El projecte utilitza múltiples tasques paral·leles per gestionar simultàniament la renderització, 
la lògica del joc i l'entrada de l'usuari:

1. **Tasca d'entrada d'usuari (`inputTask`)**:
  - S'executa en un fil separat mitjançant `Task.Run(() => gameController.HandleUserInput())`
  - Comprova constantment si hi ha tecles disponibles (`Console.KeyAvailable`)
  - Actualitza la posició del jugador en funció de les tecles premudes
  - Utilitza un bloc `lock` per garantir la seguretat en l'accés a les dades compartides
  - Incorpora petites pauses (`Thread.Sleep(5)`) per evitar un ús excessiu de CPU

2. **Tasca de lògica del joc (`gameLogicTask`)**:
  - S'executa en un altre fil amb `Task.Run(() => gameController.UpdateGameState())`
  - Actualitza l'estat del joc a una velocitat fixa (50 Hz)
  - Genera nous asteroides, mou els existents i comprova col·lisions
  - Utilitza blocs `lock` per sincronitzar les modificacions de l'estat del joc

3. **Tasca de renderització (`renderTask`)**:
  - S'executa en un tercer fil amb `Task.Run(() => uiController.RenderGame())`
  - Renderitza el joc a una velocitat específica (20 Hz)
  - Només actualitza les parts de la pantalla que han canviat
  - Utilitza el mateix objecte de bloqueig per coordinar-se amb les altres tasques

4. **Coordinació de tasques**:
  - S'utilitza `await Task.WhenAll(gameLogicTask, renderTask, inputTask)` per esperar que totes les tasques es completin abans de procedir amb la següent fase del joc
  - Això garanteix una terminació ordenada de totes les tasques quan el joc finalitza

## Has diferenciat entre programació paral·lela i asíncrona?

Sí, el projecte diferencia clarament entre la programació paral·lela i asíncrona:

### Programació paral·lela

La programació paral·lela s'utilitza per a les tasques que necessiten executar-se simultàniament i de forma contínua:

- Les tres tasques principals del joc (input, lògica i renderització) s'executen en paral·lel utilitzant `Task.Run()`
- Cadascuna s'executa en un fil separat que realitza un bucle continu mentre el joc està en execució
- S'utilitzen mecanismes de sincronització (objecte `LockObject`) per coordinar l'accés a les dades compartides
- Els diferents fils tenen freqüències d'execució diferents (20 Hz per a la renderització, 50 Hz per a la lògica del joc)

### Programació asíncrona

La programació asíncrona s'utilitza principalment per operacions no bloquejants i coordinació de tasques:

- El mètode `Main` està marcat com `async` i utilitza `await` per esperar que les tasques es completin
- `Task.WhenAll()` s'utilitza per esperar que totes les tasques finalitzin abans de procedir
- La simulació de l'avaluació web (`SimulateWebEvaluation`) utilitza `await Task.Delay()` per simular el temps d'espera sense bloquejar el fil principal
- Els efectes de so es reprodueixen en tasques separades (`Task.Run(() => Console.Beep(...))`) per evitar bloquejar la lògica del joc

Aquesta diferenciació és fonamental per al correcte funcionament del joc:
- La programació paral·lela permet que les tasques d'alta freqüència (verificació d'entrada, actualització d'estat, renderització) s'executin simultàniament
- La programació asíncrona permet que les operacions de llarga durada o menys crítiques (com els efectes de so o l'avaluació web) s'executin sense bloquejar les tasques principals

En resum, s'ha utilitzat la programació paral·lela per aconseguir simultaneïtat real en les operacions crítiques del joc, mentre que la programació asíncrona s'ha utilitzat per a la coordinació de tasques i per a operacions no bloquejants.