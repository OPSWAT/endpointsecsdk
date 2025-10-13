"""
OPSWAT Patch Management Catalog Simplifier - Data Classes
---------------------------------------------------------
This module defines the data classes used to represent vendors, products, patches, vulnerabilities, and related entities.

Author: Chris Seiler

Purpose:
- Provides structured representations for all catalog entities.
- Used throughout the catalog simplification process for type safety and serialization.
"""
from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional

@dataclass
class Vendor:
    """Represents a software vendor."""
    id: int
    name: str

    def to_dict(self) -> dict:
        return {
            "id": self.id,
            "name": self.name
        }

@dataclass
class Range:
    """Represents a version or date range."""
    start: str
    end: str

    def to_dict(self) -> dict:
        return {
            "start": self.start,
            "end": self.end
        }

@dataclass
class Product_Line:
    """Represents a product line (family) for a product."""
    id: int
    name: str

    def to_dict(self) -> dict:
        return {
            "id": self.id,
            "name": self.name
        }

@dataclass(frozen=True)
class Vulnerability:
    """Represents a vulnerability (CVE/CPE and optional ranges)."""
    cve: str
    cpe: Optional[str] = None
    ranges: Optional[List['Range']] = None

    def to_dict(self) -> dict:
        return {
            "cve": self.cve,
            "cpe": self.cpe,
            "ranges": [r.to_dict() for r in (self.ranges or [])]
        }

@dataclass(frozen=True)
class Package:
    """Represents a downloadable package for a patch."""
    url: str
    package_id: str
    name: str
    language: Optional[str] = None
    sha256: Optional[str] = None
    architecture: Optional[str] = None

    def to_dict(self) -> dict:
        out = {
            "name": self.name,
            "package_id": self.package_id,
            "url": self.url
        }
        if self.language:
            out["language"] = self.language
        if self.sha256:
            out["sha256"] = self.sha256
        if self.architecture:
            out["architecture"] = self.architecture
        return out

@dataclass
class Property:
    """Represents a key-value property for a patch or product."""
    name: str
    value: Optional[str] = None

    def to_dict(self) -> dict:
        return {
            "name": self.name,
            "value": self.value
        }

@dataclass
class Patch:
    """Represents a patch for a product."""
    name: str = ""
    patch_id: str = ""
    bulletin: Optional[str] = None
    kb_article: Optional[str] = None
    cve: List[str] = field(default_factory=list)
    latest: bool = False
    language: Optional[str] = None
    packages: List[Package] = field(default_factory=list)
    fresh_installable: Optional[bool] = None
    background_patching: Optional[bool] = None
    validation_supported: Optional[bool] = None
    properties: List[Property] = field(default_factory=list)
    version: Optional[str] = None
    release_notes: Optional[str] = None
    release_date: Optional[str] = None
    eula: Optional[str] = None
    uninstall_required: Optional[bool] = None
    reboot_required: Optional[str] = None
    architectures: List[str] = field(default_factory=list)
    language_default: Optional[str] = None
    platform: Optional[str] = None
    patch_type: Optional[str] = None
    delivery_mode: Optional[str] = None

    def to_dict(self) -> dict:
        """Convert Patch to a dictionary for JSON serialization."""
        """Convert Patch to a dictionary for JSON serialization, skipping None or 0 values."""
        data = {
            "name": self.name,
            "bulletin": self.bulletin,
            "patch_id": self.patch_id,
            "platform": self.platform,
            "patch_type": self.patch_type,
            "kb_article": self.kb_article,
            "latest": self.latest,
            "language": self.language,
            "fresh_installable": self.fresh_installable,
            "background_patching": self.background_patching,
            "validation_supported": self.validation_supported,
            "version": self.version,
            "release_notes": self.release_notes,
            "release_date": self.release_date,
            "eula": self.eula,
            "uninstall_required": self.uninstall_required,
            "reboot_required": self.reboot_required,
            "architectures": self.architectures,
            "language_default": self.language_default,
            "delivery_mode": self.delivery_mode,
            "properties": [prop.to_dict() for prop in self.properties],
            "cve": self.cve,
            "packages": [p.to_dict() for p in self.packages],
        }

        # remove any keys where the value is None or 0
        return {k: v for k, v in data.items() if v not in (None, 0)}

@dataclass
class Product:
    """Represents a product with its metadata, patches, and vulnerabilities."""
    signature_id: Optional[int] = None
    name: Optional[str] = None
    vendor: Optional[Vendor] = None
    product_line: Optional[Product_Line] = None
    marketing_names: Optional[List[str]] = None
    patches: List[Patch] = field(default_factory=list)
    vulnerabilities: List[Vulnerability] = field(default_factory=list)
    uninstallable: Optional[bool] = False
    categories: Optional[List[str]] = None

    def to_dict(self) -> dict:
        """Convert Product to a dictionary for JSON serialization."""
        result = {
            "name": self.name,
            "signature_id": self.signature_id,
        }

        if(self.product_line):
            result["product_line"] = self.product_line.to_dict() 
        if(self.vendor):
            result["vendor"] = self.vendor.to_dict()
        if self.uninstallable:
            result["uninstallable"] = self.uninstallable
        if self.marketing_names:
            result["marketing_names"] = self.marketing_names
        if self.categories:
            result["categories"] = self.categories
        if self.patches:
            result["patches"] = [patch.to_dict() for patch in self.patches]
        if self.vulnerabilities:
            result["vulnerabilities"] = [vuln.to_dict() for vuln in self.vulnerabilities]
        return result

@dataclass
class Metadata:
    """Metadata for the Product class."""
    release_date: Optional[str] = None

    def to_dict(self) -> dict:
        """Convert Meta object to a dictionary."""
        result = {
            "release_date": self.release_date
            }
            
        return result


@dataclass
class Data:
    """Holds metadata and product list."""
    meta: Optional[Metadata] = None
    products: List['Product'] = field(default_factory=list)

    def to_dict(self) -> dict:
        """Convert Data object to a dictionary."""
        # Handle nested dataclasses properly
        return {
            "meta": self.meta.to_dict() if self.meta else None,
            "products": [
    p.to_dict() if hasattr(p, "to_dict") else p
    for p in self.products
]
        }
