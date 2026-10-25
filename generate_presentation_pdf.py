import os
import sys
from reportlab.lib.pagesizes import A4
from reportlab.lib import colors
from reportlab.lib.units import cm, mm
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, Image, KeepTogether, PageBreak, HRFlowable
)
from reportlab.pdfgen import canvas
from PIL import Image as PILImage

# Paths
OUTPUT_PDF = "/home/x/Desktop/PharmaFind/PharmaFind_Presentation_Carte.pdf"
SCREENSHOT_PATH = "/home/x/.gemini/antigravity-ide/brain/aa5aaf53-524b-4d69-9f7a-9647d68b7e25/s_real_pin_tap.png"
CROPPED_SCREENSHOT = "/home/x/Desktop/PharmaFind/map_screenshot_clean.png"

# Prepare clean crop of the screenshot (focus on map & bottom card, exclude top notification bar if any)
if os.path.exists(SCREENSHOT_PATH):
    img = PILImage.open(SCREENSHOT_PATH)
    # 720 x 1612: crop status bar at top (y=60) to bottom tabs (y=1500)
    w, h = img.size
    crop_area = (0, 60, w, min(h, 1500))
    cropped = img.crop(crop_area)
    cropped.save(CROPPED_SCREENSHOT)

class NumberedCanvas(canvas.Canvas):
    """Two-pass canvas to dynamically compute and print total page count"""
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self._saved_page_states = []

    def showPage(self):
        self._saved_page_states.append(dict(self.__dict__))
        self._startPage()

    def save(self):
        num_pages = len(self._saved_page_states)
        for state in self._saved_page_states:
            self.__dict__.update(state)
            self.draw_header_footer(num_pages)
            super().showPage()
        super().save()

    def draw_header_footer(self, page_count):
        self.saveState()
        self.setFont("Helvetica", 8)
        self.setFillColor(colors.HexColor("#64748B"))
        
        # Header (pages 2+)
        if self._pageNumber > 1:
            self.drawString(18 * mm, 285 * mm, "PharmaFind — Fiche Technique : Option Cartographique")
            self.setStrokeColor(colors.HexColor("#E2E8F0"))
            self.setLineWidth(0.5)
            self.line(18 * mm, 283 * mm, 192 * mm, 283 * mm)
            
        # Footer
        footer_text = f"PharmaFind · Soutenance & Démonstration · Page {self._pageNumber} sur {page_count}"
        self.drawRightString(192 * mm, 10 * mm, footer_text)
        self.drawString(18 * mm, 10 * mm, "Confidentiel & Professionnel — .NET 10 MAUI & Leaflet OSM")
        self.setStrokeColor(colors.HexColor("#CBD5E1"))
        self.setLineWidth(0.5)
        self.line(18 * mm, 14 * mm, 192 * mm, 14 * mm)
        self.restoreState()

def build_pdf():
    doc = SimpleDocTemplate(
        OUTPUT_PDF,
        pagesize=A4,
        leftMargin=14 * mm,
        rightMargin=14 * mm,
        topMargin=14 * mm,
        bottomMargin=16 * mm
    )

    styles = getSampleStyleSheet()
    
    # Custom color palette
    c_primary = colors.HexColor("#065F46")      # Deep Emerald Green
    c_primary_light = colors.HexColor("#ECFDF5")# Light mint
    c_secondary = colors.HexColor("#1E3A8A")    # Deep Navy Blue
    c_text_dark = colors.HexColor("#0F172A")    # Slate 900
    c_text_muted = colors.HexColor("#475569")   # Slate 600
    c_accent = colors.HexColor("#0284C7")       # Sky Blue
    c_border = colors.HexColor("#CBD5E1")       # Border Slate 300
    c_warn_bg = colors.HexColor("#FFFBEB")
    c_warn_border = colors.HexColor("#F59E0B")

    # Typography styles
    style_title = ParagraphStyle(
        'DocTitle',
        parent=styles['Normal'],
        fontName='Helvetica-Bold',
        fontSize=18,
        leading=22,
        textColor=c_primary
    )

    style_subtitle = ParagraphStyle(
        'DocSubtitle',
        parent=styles['Normal'],
        fontName='Helvetica',
        fontSize=10,
        leading=14,
        textColor=c_text_muted
    )

    style_h1 = ParagraphStyle(
        'Heading1Custom',
        parent=styles['Normal'],
        fontName='Helvetica-Bold',
        fontSize=12,
        leading=16,
        textColor=c_primary,
        spaceBefore=6,
        spaceAfter=4
    )

    style_h2 = ParagraphStyle(
        'Heading2Custom',
        parent=styles['Normal'],
        fontName='Helvetica-Bold',
        fontSize=10,
        leading=13,
        textColor=c_secondary,
        spaceBefore=4,
        spaceAfter=2
    )

    style_body = ParagraphStyle(
        'BodyCustom',
        parent=styles['Normal'],
        fontName='Helvetica',
        fontSize=8.5,
        leading=11.5,
        textColor=c_text_dark
    )

    style_body_bold = ParagraphStyle(
        'BodyBoldCustom',
        parent=styles['Normal'],
        fontName='Helvetica-Bold',
        fontSize=8.5,
        leading=11.5,
        textColor=c_text_dark
    )

    style_bullet = ParagraphStyle(
        'BulletCustom',
        parent=styles['Normal'],
        fontName='Helvetica',
        fontSize=8.3,
        leading=11.5,
        textColor=c_text_dark,
        leftIndent=10
    )

    style_oral = ParagraphStyle(
        'OralPitch',
        parent=styles['Normal'],
        fontName='Helvetica',
        fontSize=8.5,
        leading=12,
        textColor=colors.HexColor("#1E293B")
    )

    story = []

    # ================= PAGE 1 =================

    # Top Banner Header Table
    header_content = [
        [
            Paragraph("<b>PharmaFind</b> — Fiche Technique pour Exposé", style_title),
            Paragraph("<b>Date :</b> Octobre 2026<br/><b>Contexte :</b> Projet Santé / Ville de Douala<br/><b>Stack :</b> .NET 10 MAUI & Leaflet OSM", style_subtitle)
        ]
    ]
    t_header = Table(header_content, colWidths=[120 * mm, 62 * mm])
    t_header.setStyle(TableStyle([
        ('VALIGN', (0,0), (-1,-1), 'MIDDLE'),
        ('BOTTOMPADDING', (0,0), (-1,-1), 4),
        ('TOPPADDING', (0,0), (-1,-1), 0),
    ]))
    story.append(t_header)

    story.append(HRFlowable(width="100%", thickness=1.5, color=c_primary, spaceBefore=4, spaceAfter=8))

    # 1. Résumé Exécutif
    story.append(Paragraph("1. VUE D'ENSEMBLE DU PROJET", style_h1))
    p_resume = (
        "<b>PharmaFind</b> est une solution mobile et cloud conçue pour localiser en temps réel la disponibilité "
        "des médicaments dans les pharmacies proches (pilote : Douala, Cameroun). Face aux ruptures régulières "
        "et aux déplacements inutiles des patients, l'application offre une recherche rapide (DCI/nom commercial) "
        "et une <b>carte interactive géolocalisée</b> indiquant l'état des stocks (Disponible, Faible, Rupture)."
    )
    story.append(Paragraph(p_resume, style_body))
    story.append(Spacer(1, 3 * mm))

    # 2. Le Choix de l'Option Cartographique (Cœur de l'exposé)
    story.append(Paragraph("2. L'OPTION CARTOGRAPHIQUE : LEAFLET.JS & OPENSTREETMAP (OSM)", style_h1))
    
    comparatif_data = [
        [
            Paragraph("<b>Critère</b>", style_body_bold),
            Paragraph("<b>Google Maps SDK (Classique)</b>", style_body_bold),
            Paragraph("<b>Leaflet.js + OpenStreetMap (Notre choix)</b>", style_body_bold)
        ],
        [
            Paragraph("<b>Coût & Dépendance</b>", style_body),
            Paragraph("Payant au-delà du quota, carte bancaire requise, risque de coupure de service.", style_body),
            Paragraph("<font color='#065F46'><b>100 % Gratuit et Open-Source</b></font>, aucune clé d'API obligatoire ni facturation imprévue.", style_body)
        ],
        [
            Paragraph("<b>Résilience Hors-ligne</b>", style_body),
            Paragraph("Lourd, dépendance forte aux serveurs Google Play Services.", style_body),
            Paragraph("<font color='#065F46'><b>Bibliothèques embarquées</b></font> (CSS et JS stockés en mémoire dans l'app). Rendu instantané même sans 4G.", style_body)
        ],
        [
            Paragraph("<b>Personnalisation</b>", style_body),
            Paragraph("Limitée par les composants natifs Google.", style_body),
            Paragraph("Plein contrôle HTML/SVG : pins animés aux couleurs des stocks, cercles de rayon, tooltips riches.", style_body)
        ],
        [
            Paragraph("<b>Pertinence Locale</b>", style_body),
            Paragraph("Parfois incomplet sur certains axes secondaires.", style_body),
            Paragraph("OSM très riche sur l'Afrique et mis à jour collaborativement (quartiers de Douala : Akwa, Bonapriso, Deido...).", style_body)
        ]
    ]
    t_comp = Table(comparatif_data, colWidths=[38 * mm, 68 * mm, 76 * mm])
    t_comp.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,0), c_primary_light),
        ('GRID', (0,0), (-1,-1), 0.5, c_border),
        ('VALIGN', (0,0), (-1,-1), 'TOP'),
        ('TOPPADDING', (0,0), (-1,-1), 3),
        ('BOTTOMPADDING', (0,0), (-1,-1), 3),
        ('LEFTPADDING', (0,0), (-1,-1), 4),
        ('RIGHTPADDING', (0,0), (-1,-1), 4),
    ]))
    story.append(t_comp)
    story.append(Spacer(1, 4 * mm))

    # 3. Architecture Technique du Pont Carte (WebView <-> C# .NET MAUI)
    story.append(Paragraph("3. ARCHITECTURE : LE PONT INTERACTIF HYBRIDE (C# / JAVASCRIPT)", style_h1))
    p_arch = (
        "La carte fonctionne au sein d'une <b>WebView native .NET MAUI</b> hautement optimisée. "
        "La communication bidirectionnelle entre le monde Web (Leaflet) et le monde C# (MAUI) repose sur "
        "une architecture par interception d'événements : "
    )
    story.append(Paragraph(p_arch, style_body))
    story.append(Spacer(1, 1.5 * mm))

    arch_steps = [
        [
            Paragraph("<b>Flux</b>", style_body_bold),
            Paragraph("<b>Mécanisme Technique</b>", style_body_bold),
            Paragraph("<b>Impact Utilisateur</b>", style_body_bold)
        ],
        [
            Paragraph("<b>C# → Carte</b>", style_body),
            Paragraph("<code>LeafletMapProvider.BuildHtml(MapModel)</code> génère le code HTML injectant les coordonnées GPS, le rayon (1 à 20 km) et les pharmacies filtrées.", style_body),
            Paragraph("Centrage automatique sur Douala avec zoom adapté et affichage des marqueurs.", style_body)
        ],
        [
            Paragraph("<b>Carte → C#</b>", style_body),
            Paragraph("Au clic sur un marqueur, JavaScript appelle <code>window.location.href = 'https://local.pharmafind/pharmacy/{id}?_t=...'</code>.", style_body),
            Paragraph("Le protocole HTTPS local est intercepté par <code>OnMapNavigating</code> dans MAUI qui annule la navigation Web.", style_body)
        ],
        [
            Paragraph("<b>Affichage Réactif</b>", style_body),
            Paragraph("MAUI appelle <code>_vm.OnMarkerSelected(id)</code> qui lie immédiatement la pharmacie sélectionnée sans recharger la WebView.", style_body),
            Paragraph("<font color='#065F46'><b>Fiche basse native</b></font> surgissant instantanément avec boutons <i>Appeler</i>, <i>Itinéraire</i> et <i>Détails</i>.", style_body)
        ]
    ]
    t_arch = Table(arch_steps, colWidths=[28 * mm, 82 * mm, 72 * mm])
    t_arch.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,0), colors.HexColor("#F1F5F9")),
        ('GRID', (0,0), (-1,-1), 0.5, c_border),
        ('VALIGN', (0,0), (-1,-1), 'TOP'),
        ('TOPPADDING', (0,0), (-1,-1), 2.5),
        ('BOTTOMPADDING', (0,0), (-1,-1), 2.5),
        ('LEFTPADDING', (0,0), (-1,-1), 4),
        ('RIGHTPADDING', (0,0), (-1,-1), 4),
    ]))
    story.append(t_arch)

    # Force page break to guarantee strict 2-page layout
    story.append(PageBreak())

    # ================= PAGE 2 =================

    story.append(Paragraph("4. DÉMONSTRATION EN CONDITIONS RÉELLES & EXPÉRIENCE UTILISATEUR", style_h1))
    
    # Dual column layout: Left = Explanations, Right = Live Phone Screenshot
    left_content = [
        Paragraph("<b>Code couleur d'état des stocks :</b>", style_body_bold),
        Paragraph("• <font color='#15803D'><b>Vert (#15803D)</b></font> : Médicament disponible en rayon.<br/>"
                  "• <font color='#D97706'><b>Orange (#D97706)</b></font> : Stock faible (moins de 5 boîtes).<br/>"
                  "• <font color='#B91C1C'><b>Rouge (#B91C1C)</b></font> : Rupture déclarée.<br/>"
                  "• <font color='#6B7280'><b>Gris (#6B7280)</b></font> : Disponibilité non confirmée récemment.", style_body),
        Spacer(1, 2 * mm),
        Paragraph("<b>Actions directes depuis la carte :</b>", style_body_bold),
        Paragraph("• <b>Appel en 1 clic :</b> Déclenche le composeur Android pour vérifier de vive voix.<br/>"
                  "• <b>Itinéraire GPS :</b> Ouvre Google Maps / Waze avec guidage virage par virage.<br/>"
                  "• <b>Fiche détaillée :</b> Affiche l'inventaire complet déclaré par la pharmacie, le prix en FCFA et les horaires 24h/24.", style_body),
        Spacer(1, 3 * mm),
        Paragraph("<b>5. PROBLÈMES TECHNIQUES SURMONTÉS AVEC SUCCÈS :</b>", style_h1),
        Paragraph("1. <b>Écran blanc au changement d'onglet résolu :</b> Forçage de la réaffectation de la source HTML dans <code>OnAppearing()</code> et invalidation de taille (<code>invalidateSize</code>) à l'affichage.<br/>"
                  "2. <b>Suppression du blocage visuel (spinner infini) :</b> Remplacement de l'état bloquant plein écran par une recherche fluide asynchrone en arrière-plan avec indicateur discret.<br/>"
                  "3. <b>Fiabilité tactile Android 14 :</b> Correction du bug MAUI où les sous-vues absorbaient les clics, par l'utilisation de liaisons natives <code>SelectionMode='Single'</code>.", style_body)
    ]

    # Check if cropped screenshot exists
    img_element = []
    if os.path.exists(CROPPED_SCREENSHOT):
        img_element.append(Image(CROPPED_SCREENSHOT, width=62 * mm, height=138 * mm))
        img_element.append(Paragraph("<font size='7' color='#64748B'><i>Capture réelle sur smartphone TECNO BG6m</i></font>", ParagraphStyle('cap', parent=styles['Normal'], alignment=1)))
    else:
        img_element.append(Paragraph("Screenshot non disponible", style_body))

    demo_table_data = [
        [
            left_content,
            img_element
        ]
    ]
    t_demo = Table(demo_table_data, colWidths=[118 * mm, 64 * mm])
    t_demo.setStyle(TableStyle([
        ('VALIGN', (0,0), (-1,-1), 'TOP'),
        ('LEFTPADDING', (0,0), (-1,-1), 0),
        ('RIGHTPADDING', (0,0), (-1,-1), 0),
        ('TOPPADDING', (0,0), (-1,-1), 0),
        ('BOTTOMPADDING', (0,0), (-1,-1), 0),
    ]))
    story.append(t_demo)

    story.append(Spacer(1, 3 * mm))

    # 6. PITCH ORAL POUR L'EXPOSÉ (Cheat-sheet pour parler devant le jury)
    story.append(Paragraph("6. GUIDE D'EXPOSÉ ORAL (À DIRE EN 2 MINUTES DEVANT LE JURY)", style_h1))
    
    pitch_boxes = [
        [
            Paragraph(
                "<b>1. Introduction (30 sec) :</b><br/>"
                "« <i>Bonjour. Pour résoudre le problème des ruptures de médicaments à Douala, nous avons créé PharmaFind. "
                "L'élément central pour le patient est la carte interactive, qui évite les allers-retours inutiles dans les embouteillages.</i> »",
                style_oral
            )
        ],
        [
            Paragraph(
                "<b>2. Pourquoi cette solution cartographique (45 sec) :</b><br/>"
                "« <i>Nous avons fait le choix technique de Leaflet et OpenStreetMap plutôt que Google Maps : d'abord pour l'indépendance financière "
                "et l'open-source (0 FCFA de coût d'API). Ensuite pour la résilience : les moteurs cartographiques sont embarqués dans l'application, "
                "ce qui permet à l'interface de s'afficher instantanément même avec une connectivité réseau instable.</i> »",
                style_oral
            )
        ],
        [
            Paragraph(
                "<b>3. Démonstration & Ergonomie (45 sec) :</b><br/>"
                "« <i>Sur le plan technique, nous avons implémenté un pont hybride C#/JavaScript sans faille. "
                "Chaque marqueur reflète l'état réel du stock par sa couleur. Toucher une pharmacie fait surgir sa fiche avec distance en km, "
                "prix en FCFA et déclenchement d'appel ou de guidage GPS en un seul toucher. Tout a été testé et validé sur appareil réel.</i> »",
                style_oral
            )
        ]
    ]
    t_pitch = Table(pitch_boxes, colWidths=[182 * mm])
    t_pitch.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,-1), c_primary_light),
        ('BOX', (0,0), (-1,-1), 1, c_primary),
        ('VALIGN', (0,0), (-1,-1), 'MIDDLE'),
        ('TOPPADDING', (0,0), (-1,-1), 3),
        ('BOTTOMPADDING', (0,0), (-1,-1), 3),
        ('LEFTPADDING', (0,0), (-1,-1), 6),
        ('RIGHTPADDING', (0,0), (-1,-1), 6),
    ]))
    story.append(t_pitch)

    # Build the document using the custom NumberedCanvas
    doc.build(story, canvasmaker=NumberedCanvas)
    print(f"PDF generated successfully at: {OUTPUT_PDF}")

if __name__ == "__main__":
    build_pdf()
