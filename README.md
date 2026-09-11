# ACHTUNG!
Aktuell ist es noch eine Beta und kann Fehler enthalten. Versioniertes Backup eures Lightroom-Katalogs ist immer zu empfehlen.

# LrCatalogSync

- Das LrCatalogSync-Programm synchronisiert den Katalog samt Hilfsdateien von Adobe Lightroom Classic auf einen Samba-Server.
- Es erkennt, wenn ein Sync oder Lightroom Classic von einem anderen Rechner läuft, und verhindert das lokale Öffnen des Lightroom-Classic-Katalogs sowie den eigenen Sync-Prozess, um die Datenkonsistenz zu erhalten.
- LrCatalogSync startet mit Windows automatisch und ist über ein Symbol in der Taskleiste zu finden.

## Funktionsweise

### Kopierte Dateien
Beim Sync werden folgende Lightroom‑Dateien und Ordner synchronisiert:
- `*.lrcat` – Die Hauptkatalogdatei (SQL)
- `*.lrcat-data/` – Katalog-Datenbank (Masken, KI-Auswahlen)
- `* Sync.lrdata/` – Für Adobe Creative Cloud
- `* Smart Previews.lrdata/` – kleine Vorschaudateien von Raw/DNG
- `* Helper.lrdata/` – Hilfsdaten für Katalogfunktionen

Optional (siehe Einstellungen):
- `* Previews.lrdata/` – Standard und 1:1 Vorschaudateien 
- `Katalog Backups *.zip` - Automatische Sicherungsdaten von Lightroom

### Lock-Dateien (Lightroom-Erkennung)
Das Programm erkennt automatisch, wenn Lightroom geöffnet ist, und verzichtet dann auf den Sync:
- `*.lrcat.lock` – Haupt-Lock-Datei
- `*.lrcat-shm` – Shared Memory Segment
- `*.lrcat-wal` – Write-Ahead Log

Diese Dateien werden von Lightroom Classic beim Öffnen des Katalogs erstellt und beim Schließen wieder gelöscht.

## Voraussetzungen
- ab Windows 8.1
- **rclone** (https://rclone.org)

## Installation
1. rclone herunterladen, `rclone.exe` z. B. nach `C:\Programme\rclone` entpacken.
2. LrCatalogSync von GitHub herunterladen und `LrCatalogSync.exe` starten – das Symbol erscheint im Tray.

## Nutzung
*Start:* Doppelklick auf `LrCatalogSync.exe` (kann beim Systemstart aktiviert werden). 

*Stop:* Rechtsklick auf das Tray‑Icon → **Beenden**.

## Konfiguration (grafisch)
![alt text](docs/images/config_menu.png)
| Feld | Beschreibung |
|------|--------------|
| **Auto-Start** | Programm beim Windows-Start automatisch ausführen |
| **rclone‑Pfad** | Pfad zur `rclone.exe` (z. B. `C:\Programme\rclone\rclone.exe`) |
| **Log‑Level** | `DEBUG`, `INFO`, `NOTICE`, `ERROR` |
| **Aktualisierungszeit** | Wie oft pro Sekunde überprüft werden soll |
| **.Previews.lrdata** | Auswahl, ob 1:1-Vorschaubilder auch synchronisiert werden sollen |
| **Katalog‑Datei** | Pfad zur `.lrcat`‑Datei (lokal) |
| **Remote‑Pfad** | Zielpfad auf dem SMB‑Server (z. B. `//192.168.1.1/Lightroom/` -> `/Lightroom/`) |
| **letzten Katalog behalten?** | Speichert vor dem Sync den Katalog in einem Extra-Ordner |
| **Ordnername** | Für die letzte Katalogspeicherung |
| **Lokaler Backup Pfad** | Lokaler Pfad, wo die Sicherungsdateien von Lightroom liegen |
| **Remote Backup Pfad** | Zielpfad auf dem SMB-Server für die Sicherungsdateien |
| **Server‑IP / Host** | IP oder Hostname des SMB‑Servers |
| **Benutzer / Passwort** | Zugangsdaten (verschlüsselt gespeichert) |
| **Backup aktivieren** | Optional, lokale und Remote‑Backups synchronisieren |

Einstellungen werden in `data/config/` gespeichert.

## TrayIcon

Tray‑Icon‑Status:
- 🟢 --> bereit, kein Sync aktiv
- 🟠  --> Synchronisiere Lightroom-Sicherungsordner
- 🟡  --> Synchronisiere Lightroom-Katalog 
- 🔵  --> Lightroom Classic ist lokal aktiv und Sync wird blockiert
- 🔵 --> Wenn der PC während des Syncs neugestartet wurde, wird dieser Recovery-Prozess gestartet
- 🟣  --> Ein Sync läuft gerade von einem anderen Rechner
- 🟣 --> Lightroom Classic wurde auf einem anderen Rechner gestartet
- 🔴 --> Fehler, siehe Log (Notfalls auf Debug stellen)
- ⚪ --> Konfigurationsdatei fehlt

Logs finden Sie unter `data/logs/`.

## Fehlersuche (Kurz)
- *rclone.exe nicht gefunden*: Pfad prüfen.
- *Samba‑Verbindung fehlgeschlagen*: IP, Benutzer, Passwort und Netzwerk prüfen.

## Ressourcen
- GitHub: https://github.com/Raychan87/LrCatalogSync
- rclone: https://rclone.org
- Lightroom Classic: https://www.adobe.com/de/products/photoshop-lightroom-classic.html

*Version **0.9.10-beta** – Stand: September 2026*

