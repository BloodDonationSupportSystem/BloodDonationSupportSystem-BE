using BusinessObjects.Dtos;
using Microsoft.Extensions.Logging;
using Repositories.Base;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class DocumentSeedService : IDocumentSeedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DocumentSeedService> _logger;

        public DocumentSeedService(
            IUnitOfWork unitOfWork,
            ILogger<DocumentSeedService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse> SeedBloodCompatibilityDocumentsAsync(Guid adminUserId)
        {
            try
            {
                // Check if admin user exists
                var adminUser = await _unitOfWork.Users.GetByIdAsync(adminUserId);
                if (adminUser == null)
                {
                    return new ApiResponse(
                        HttpStatusCode.BadRequest,
                        "Admin user not found");
                }

                // Create a document for whole blood compatibility
                var wholeBloodDocument = new CreateDocumentDto
                {
                    Title = "Blood Type Compatibility Guide for Whole Blood Transfusions",
                    Content = GetWholeBloodCompatibilityContent(),
                    DocumentType = "Blood Compatibility Guide",
                    CreatedBy = adminUserId
                };

                // Create a document for red blood cells compatibility
                var redCellsDocument = new CreateDocumentDto
                {
                    Title = "Blood Type Compatibility Guide for Red Blood Cells Transfusions",
                    Content = GetRedCellsCompatibilityContent(),
                    DocumentType = "Blood Compatibility Guide",
                    CreatedBy = adminUserId
                };

                // Create a document for plasma compatibility
                var plasmaDocument = new CreateDocumentDto
                {
                    Title = "Blood Type Compatibility Guide for Plasma Transfusions",
                    Content = GetPlasmaCompatibilityContent(),
                    DocumentType = "Blood Compatibility Guide",
                    CreatedBy = adminUserId
                };

                // Create a document for platelets compatibility
                var plateletsDocument = new CreateDocumentDto
                {
                    Title = "Blood Type Compatibility Guide for Platelets Transfusions",
                    Content = GetPlateletsCompatibilityContent(),
                    DocumentType = "Blood Compatibility Guide",
                    CreatedBy = adminUserId
                };

                // Save the documents
                await _unitOfWork.Documents.AddAsync(MapToDocument(wholeBloodDocument));
                await _unitOfWork.Documents.AddAsync(MapToDocument(redCellsDocument));
                await _unitOfWork.Documents.AddAsync(MapToDocument(plasmaDocument));
                await _unitOfWork.Documents.AddAsync(MapToDocument(plateletsDocument));
                
                // Save changes to database
                await _unitOfWork.CompleteAsync();

                return new ApiResponse(
                    "Blood compatibility documents created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding blood compatibility documents");
                return new ApiResponse(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while seeding blood compatibility documents");
            }
        }

        private BusinessObjects.Models.Document MapToDocument(CreateDocumentDto dto)
        {
            return new BusinessObjects.Models.Document
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Content = dto.Content,
                DocumentType = dto.DocumentType,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTimeOffset.UtcNow
            };
        }

        private string GetWholeBloodCompatibilityContent()
        {
            return @"<h1>Whole Blood Transfusion Compatibility</h1>
<p>Understanding blood type compatibility is crucial for safe blood transfusions. Blood types are determined by the presence or absence of certain antigens on the surface of red blood cells, primarily the A, B, and Rh antigens.</p>

<h2>ABO Blood Group System</h2>
<p>The ABO system classifies blood into four main types:</p>
<ul>
  <li><strong>Type A:</strong> Has A antigens on red cells and anti-B antibodies in plasma</li>
  <li><strong>Type B:</strong> Has B antigens on red cells and anti-A antibodies in plasma</li>
  <li><strong>Type AB:</strong> Has both A and B antigens on red cells but no antibodies against A or B in plasma</li>
  <li><strong>Type O:</strong> Has neither A nor B antigens on red cells but both anti-A and anti-B antibodies in plasma</li>
</ul>

<h2>Rh Factor</h2>
<p>The Rh (Rhesus) factor is another important blood group system:</p>
<ul>
  <li><strong>Rh-positive (Rh+):</strong> Has the Rh antigen on red cells</li>
  <li><strong>Rh-negative (Rh-):</strong> Does not have the Rh antigen on red cells</li>
</ul>

<h2>Compatibility Chart for Whole Blood Transfusions</h2>
<table border='1' cellpadding='5'>
  <tr>
    <th>Blood Type</th>
    <th>Can Receive From</th>
    <th>Can Donate To</th>
  </tr>
  <tr>
    <td>O-</td>
    <td>O-</td>
    <td>All blood types (Universal donor)</td>
  </tr>
  <tr>
    <td>O+</td>
    <td>O-, O+</td>
    <td>O+, A+, B+, AB+</td>
  </tr>
  <tr>
    <td>A-</td>
    <td>O-, A-</td>
    <td>A-, A+, AB-, AB+</td>
  </tr>
  <tr>
    <td>A+</td>
    <td>O-, O+, A-, A+</td>
    <td>A+, AB+</td>
  </tr>
  <tr>
    <td>B-</td>
    <td>O-, B-</td>
    <td>B-, B+, AB-, AB+</td>
  </tr>
  <tr>
    <td>B+</td>
    <td>O-, O+, B-, B+</td>
    <td>B+, AB+</td>
  </tr>
  <tr>
    <td>AB-</td>
    <td>O-, A-, B-, AB-</td>
    <td>AB-, AB+</td>
  </tr>
  <tr>
    <td>AB+</td>
    <td>All blood types (Universal recipient)</td>
    <td>AB+</td>
  </tr>
</table>

<h2>Important Notes</h2>
<p>For whole blood transfusions, both ABO and Rh compatibility must be considered. In emergency situations where fully matched blood is not available, medical professionals may use blood products with partial compatibility, but this carries increased risks.</p>

<p>Type O- individuals are considered universal donors for red blood cells, as their blood lacks A, B, and Rh antigens. Type AB+ individuals are considered universal recipients, as they have no antibodies against A, B, or Rh antigens.</p>

<h2>Clinical Implications</h2>
<p>Transfusing incompatible blood can cause severe, potentially fatal hemolytic transfusion reactions. Always consult with healthcare professionals for proper blood type matching and compatibility verification before any blood transfusion.</p>";
        }

        private string GetRedCellsCompatibilityContent()
        {
            return @"<h1>Red Blood Cells (RBC) Transfusion Compatibility</h1>
<p>Red blood cell transfusions are given to increase oxygen delivery to tissues. For RBC transfusions, compatibility focuses primarily on avoiding reactions between recipient antibodies and donor antigens.</p>

<h2>RBC Compatibility Principles</h2>
<p>When transfusing red blood cells (packed red cells, PRBCs):</p>
<ul>
  <li>The donor's red cell antigens must be compatible with the recipient's antibodies</li>
  <li>The recipient's antibodies will react with incompatible donor antigens</li>
  <li>The donor's plasma antibodies are typically not a concern as most of the plasma is removed in PRBCs</li>
</ul>

<h2>Compatibility Chart for Red Blood Cell Transfusions</h2>
<table border='1' cellpadding='5'>
  <tr>
    <th>Recipient Blood Type</th>
    <th>Compatible Red Blood Cell Donors</th>
  </tr>
  <tr>
    <td>O-</td>
    <td>O-</td>
  </tr>
  <tr>
    <td>O+</td>
    <td>O-, O+</td>
  </tr>
  <tr>
    <td>A-</td>
    <td>O-, A-</td>
  </tr>
  <tr>
    <td>A+</td>
    <td>O-, O+, A-, A+</td>
  </tr>
  <tr>
    <td>B-</td>
    <td>O-, B-</td>
  </tr>
  <tr>
    <td>B+</td>
    <td>O-, O+, B-, B+</td>
  </tr>
  <tr>
    <td>AB-</td>
    <td>O-, A-, B-, AB-</td>
  </tr>
  <tr>
    <td>AB+</td>
    <td>O-, O+, A-, A+, B-, B+, AB-, AB+</td>
  </tr>
</table>

<h2>Clinical Considerations</h2>
<p>For red blood cell transfusions:</p>
<ul>
  <li><strong>Type O-</strong> red cells can be given to patients of all blood types in emergency situations when there is no time for blood typing. These donors are called 'universal red cell donors'.</li>
  <li><strong>Type AB+</strong> patients can receive red cells from all blood types and are considered 'universal red cell recipients'.</li>
  <li>Matching Rh factor is particularly important for females of childbearing age to prevent Rh sensitization.</li>
</ul>

<h2>Special Considerations</h2>
<p>While the above chart represents standard compatibility, clinical situations may require more specific matching:</p>
<ul>
  <li>Extended antigen matching may be needed for patients receiving multiple transfusions</li>
  <li>Patients with certain medical conditions may require specially processed or matched units</li>
  <li>In cases of rare blood types or multiple antibodies, consultation with blood bank specialists is essential</li>
</ul>";
        }

        private string GetPlasmaCompatibilityContent()
        {
            return @"<h1>Plasma Transfusion Compatibility</h1>
<p>Plasma contains antibodies but no red blood cell antigens. For plasma transfusions, compatibility is essentially the reverse of red cell compatibility, focusing on the donor's antibodies and the recipient's antigens.</p>

<h2>Plasma Compatibility Principles</h2>
<p>When transfusing plasma products (FFP, cryoprecipitate, etc.):</p>
<ul>
  <li>The donor's antibodies must be compatible with the recipient's red cell antigens</li>
  <li>The donor's antibodies can react with the recipient's red cell antigens</li>
  <li>The ABO compatibility is reversed compared to red cell transfusions</li>
</ul>

<h2>Compatibility Chart for Plasma Transfusions</h2>
<table border='1' cellpadding='5'>
  <tr>
    <th>Recipient Blood Type</th>
    <th>Compatible Plasma Donors</th>
  </tr>
  <tr>
    <td>O-</td>
    <td>All blood types (O-, O+, A-, A+, B-, B+, AB-, AB+)</td>
  </tr>
  <tr>
    <td>O+</td>
    <td>O+, A+, B+, AB+</td>
  </tr>
  <tr>
    <td>A-</td>
    <td>A-, A+, AB-, AB+</td>
  </tr>
  <tr>
    <td>A+</td>
    <td>A+, AB+</td>
  </tr>
  <tr>
    <td>B-</td>
    <td>B-, B+, AB-, AB+</td>
  </tr>
  <tr>
    <td>B+</td>
    <td>B+, AB+</td>
  </tr>
  <tr>
    <td>AB-</td>
    <td>AB-, AB+</td>
  </tr>
  <tr>
    <td>AB+</td>
    <td>AB+</td>
  </tr>
</table>

<h2>Clinical Considerations</h2>
<p>For plasma transfusions:</p>
<ul>
  <li><strong>Type AB</strong> plasma can be given to patients of all blood types because it contains no anti-A or anti-B antibodies. AB donors are considered 'universal plasma donors'.</li>
  <li><strong>Type O</strong> patients can receive plasma from all blood types and are considered 'universal plasma recipients'.</li>
  <li>The Rh factor is generally not a concern for plasma transfusions since plasma does not contain red blood cells with Rh antigens.</li>
</ul>

<h2>Plasma Products</h2>
<p>Common plasma products include:</p>
<ul>
  <li><strong>Fresh Frozen Plasma (FFP):</strong> Contains all coagulation factors and is used to correct multiple coagulation deficiencies</li>
  <li><strong>Cryoprecipitate:</strong> Rich in factor VIII, fibrinogen, and von Willebrand factor</li>
  <li><strong>Plasma Derivatives:</strong> Such as albumin, immune globulins, and coagulation factor concentrates</li>
</ul>

<p>Always consult with healthcare professionals for proper plasma product selection and compatibility verification before any transfusion.</p>";
        }

        private string GetPlateletsCompatibilityContent()
        {
            return @"<h1>Platelets Transfusion Compatibility</h1>
<p>Platelet transfusions are given to prevent or treat bleeding in patients with thrombocytopenia or platelet dysfunction. Compatibility considerations for platelets are more complex than for other blood components.</p>

<h2>Platelet Compatibility Principles</h2>
<p>When transfusing platelets:</p>
<ul>
  <li>ABO matching is preferred but not absolutely required</li>
  <li>Platelets express ABO antigens but in lower quantities than red blood cells</li>
  <li>Rh matching is important, especially for females of childbearing age</li>
  <li>HLA (Human Leukocyte Antigen) matching may be necessary for patients who are refractory to random donor platelets</li>
</ul>

<h2>Compatibility Chart for Platelet Transfusions</h2>
<table border='1' cellpadding='5'>
  <tr>
    <th>Recipient Blood Type</th>
    <th>Preferred Platelet Donors</th>
    <th>Acceptable Platelet Donors (if preferred not available)</th>
  </tr>
  <tr>
    <td>O-</td>
    <td>O-</td>
    <td>O+, A-, B-, AB-</td>
  </tr>
  <tr>
    <td>O+</td>
    <td>O+, O-</td>
    <td>A+, A-, B+, B-, AB+, AB-</td>
  </tr>
  <tr>
    <td>A-</td>
    <td>A-</td>
    <td>A+, AB-</td>
  </tr>
  <tr>
    <td>A+</td>
    <td>A+, A-</td>
    <td>AB+, AB-</td>
  </tr>
  <tr>
    <td>B-</td>
    <td>B-</td>
    <td>B+, AB-</td>
  </tr>
  <tr>
    <td>B+</td>
    <td>B+, B-</td>
    <td>AB+, AB-</td>
  </tr>
  <tr>
    <td>AB-</td>
    <td>AB-</td>
    <td>AB+</td>
  </tr>
  <tr>
    <td>AB+</td>
    <td>AB+, AB-</td>
    <td>N/A</td>
  </tr>
</table>

<h2>Special Considerations for Platelet Transfusions</h2>
<p>Several factors can affect platelet transfusion efficacy:</p>
<ul>
  <li><strong>Platelet Refractoriness:</strong> Patients may develop antibodies against platelet antigens, reducing the effectiveness of transfusions</li>
  <li><strong>HLA-Matched Platelets:</strong> May be needed for patients who are refractory to random donor platelets</li>
  <li><strong>Crossmatched Platelets:</strong> Testing donor platelets against the recipient's serum can identify compatible units</li>
  <li><strong>Leukoreduced Platelets:</strong> Removal of white blood cells can reduce the risk of alloimmunization and febrile reactions</li>
</ul>

<h2>Platelet Products</h2>
<p>Platelets can be obtained from:</p>
<ul>
  <li><strong>Random Donor Platelets:</strong> Pooled from multiple whole blood donations</li>
  <li><strong>Single Donor Platelets:</strong> Collected from one donor using apheresis technology</li>
  <li><strong>HLA-Matched Platelets:</strong> Specially selected for patients with platelet refractoriness</li>
</ul>

<p>The decision about which platelet product to use depends on patient-specific factors, availability, and clinical urgency. Always consult with transfusion medicine specialists for complex cases.</p>";
        }
    }
}