/*
 * Copyright (C) 2015  Andoni Morales Alastruey <ylatuya@gmail.com>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
 *
 */

#include "lgm-device.h"
#include <gst/interfaces/propertyprobe.h>

static LgmDeviceVideoFormat *
lgm_device_video_format_new (gint width, gint height, gint fps_n, gint fps_d)
{
  LgmDeviceVideoFormat *format;

  format = g_new0 (LgmDeviceVideoFormat, 1);
  format->width = width;
  format->height = height;
  format->fps_n = fps_n;
  format->fps_d = fps_d;

  return format;
}

static void
lgm_device_video_format_free (LgmDeviceVideoFormat * format)
{
  g_free (format);
}

static gint
lgm_device_video_format_compare (LgmDeviceVideoFormat * f1,
    LgmDeviceVideoFormat * f2)
{
  if (f1->height != f2->height) {
    return f2->height - f1->height;
  } else if ((f1->fps_n != f2->fps_n) || (f1->fps_d != f2->fps_d)) {
    gfloat r1, r2;

    r2 = (gfloat) f2->fps_n / f2->fps_d;
    r1 = (gfloat) f1->fps_n / f1->fps_d;
    return (gint) (r2 - r1);
  } else if (f1->width != f2->width) {
    return f2->width - f1->width;
  } else {
    return 0;
  }
}

gchar *
lgm_device_video_format_to_string (LgmDeviceVideoFormat * format)
{
  return g_strdup_printf ("%dx%d@%d/%d", format->width, format->height,
      format->fps_n, format->fps_d);
}

void
lgm_device_video_format_get_info (LgmDeviceVideoFormat * format,
    gint * width, gint * height, gint * fps_n, gint * fps_d)
{
  *width = format->width;
  *height = format->height;
  *fps_n = format->fps_n;
  *fps_d = format->fps_d;
}

LgmDevice *
lgm_device_new (const gchar * source_name, const gchar * device_name,
    LgmDeviceType type)
{
  LgmDevice *device;

  device = g_new0 (LgmDevice, 1);
  device->source_name = g_strdup (source_name);
  device->device_name = g_strdup (device_name);
  device->type = type;
  device->formats = NULL;

  return device;
}

void
lgm_device_free (LgmDevice * device)
{
  if (device->source_name != NULL) {
    g_free (device->source_name);
    device->source_name = NULL;
  }
  if (device->device_name != NULL) {
    g_free (device->device_name);
    device->device_name = NULL;
  }
  if (device->formats != NULL) {
    g_list_free_full (device->formats,
        (GDestroyNotify) lgm_device_video_format_free);
    device->formats = NULL;
  }
  g_free (device);
}

GList *
lgm_device_get_formats (LgmDevice * device)
{
  return device->formats;
}

gchar *
lgm_device_get_source_name (LgmDevice * device)
{
  return g_strdup (device->source_name);
}

gchar *
lgm_device_get_device_name (LgmDevice * device)
{
  return g_strdup (device->device_name);
}

static int
lgm_device_fixate_int_value (const GValue * val)
{
  int ret;

  if (G_VALUE_TYPE (val) == GST_TYPE_INT_RANGE) {
    ret = gst_value_get_int_range_min (val);
  } else if (G_VALUE_TYPE (val) == GST_TYPE_ARRAY) {
    const GValue *kid = gst_value_array_get_value (val, 0);
    ret = g_value_get_int (kid);
  } else if (G_VALUE_TYPE (val) == GST_TYPE_LIST) {
    const GValue *kid = gst_value_list_get_value (val, 0);
    ret = g_value_get_int (kid);
  } else {
    ret = g_value_get_int (val);
  }

  /* For sources returning template caps set width and height to 0 */
  if (ret == 1) {
    ret = 0;
  }
  return ret;
}

static void
lgm_device_add_format (GHashTable * table, int width, int height,
    gint fps_n, gint fps_d)
{
  LgmDeviceVideoFormat *format;
  gchar *format_str;

  format = lgm_device_video_format_new (width, height, fps_n, fps_d);
  format_str = lgm_device_video_format_to_string (format);
  if (!g_hash_table_contains (table, format_str)) {
    g_hash_table_insert (table, format_str, format);
    GST_DEBUG ("Adding format: %s\n", format_str);
  } else {
    g_free (format_str);
    lgm_device_video_format_free (format);
  }
}

static void
lgm_device_add_format_from_fps_val (GHashTable * table, int width, int height,
    const GValue * val)
{
  gint fps_n, fps_d;

  fps_n = gst_value_get_fraction_numerator (val);
  fps_d = gst_value_get_fraction_denominator (val);
  if (fps_n == 0) {
    fps_d = 0;
  }
  lgm_device_add_format (table, width, height, fps_n, fps_d);
}

static void
lgm_device_parse_structure (GstStructure * s, GHashTable * table)
{
  gint width, height;
  const GValue *val;
  gchar *struct_str;

  struct_str = gst_structure_to_string (s);
  GST_DEBUG ("Parsing structure: %s\n", struct_str);
  g_free (struct_str);

  width = lgm_device_fixate_int_value (gst_structure_get_value (s, "width"));
  height = lgm_device_fixate_int_value (gst_structure_get_value (s, "height"));

  val = gst_structure_get_value (s, "framerate");
  if (G_VALUE_TYPE (val) == GST_TYPE_FRACTION) {
    lgm_device_add_format_from_fps_val (table, width, height, val);
  } else if (G_VALUE_TYPE (val) == GST_TYPE_FRACTION_RANGE) {
    const GValue *fr_val;

    /* For sources returning template caps or ranges set framerate to 0/0 */
    lgm_device_add_format (table, width, height, 0, 0);
  } else if (G_VALUE_TYPE (val) == GST_TYPE_ARRAY) {
    guint n, len;

    len = gst_value_array_get_size (val);
    for (n = 0; n < len; n++) {
      const GValue *kid = gst_value_array_get_value (val, n);
      lgm_device_add_format_from_fps_val (table, width, height, kid);
    }
  } else if (G_VALUE_TYPE (val) == GST_TYPE_LIST) {
    guint n, len;

    len = gst_value_list_get_size (val);
    for (n = 0; n < len; n++) {
      const GValue *kid = gst_value_list_get_value (val, n);
      lgm_device_add_format_from_fps_val (table, width, height, kid);
    }
  }
}

static void
lgm_device_fill_formats (LgmDevice * device, gchar * prop_name)
{
  GstCaps *source_caps, *caps;
  GstElement *source;
  GstPad *pad;
  GHashTable *table;
  gint i;

  source = gst_element_factory_make (device->source_name, NULL);
  if (!g_strcmp0 (device->source_name, "decklinkvideosrc")) {
    g_object_set (source, prop_name, 0, NULL);
  } else {
    g_object_set (source, prop_name, device->device_name, NULL);
  }
  gst_element_set_state (source, GST_STATE_READY);
  gst_element_get_state (source, NULL, NULL, 5 * GST_SECOND);
  pad = gst_element_get_static_pad (source, "src");
  source_caps = gst_pad_get_caps_reffed (pad);
  caps = gst_caps_copy (source_caps);
  gst_caps_unref (source_caps);

  table = g_hash_table_new_full (g_str_hash, g_str_equal, g_free, NULL);

  GST_DEBUG ("Filling formats for source:%s device:%s", device->source_name,
      device->device_name);
  if (!g_strcmp0 (device->source_name, "decklinkvideosrc")) {
    lgm_device_add_format (table, 0, 0, 0, 0);
  } else {
    for (i = 0; i < gst_caps_get_size (caps); i++) {
      GstStructure *s;

      s = gst_caps_get_structure (caps, i);
      if (gst_structure_has_name (s, "video/x-raw-yuv") ||
          gst_structure_has_name (s, "video/x-raw-rgb")) {
        lgm_device_parse_structure (s, table);
      } else if (gst_structure_has_name (s, "video/x-dv")) {
        lgm_device_add_format (table, 0, 0, 0, 0);
      }
    }
  }
  device->formats = g_hash_table_get_values (table);
  device->formats = g_list_sort (device->formats,
      (GCompareFunc) lgm_device_video_format_compare);
  g_hash_table_unref (table);

  gst_element_set_state (source, GST_STATE_NULL);
  gst_element_get_state (source, NULL, NULL, 5 * GST_SECOND);
  gst_object_unref (pad);
  gst_caps_unref (caps);
}

GList *
lgm_device_enum_devices (const gchar * source_name, LgmDeviceType type)
{
  GstElement *source;
  GstPropertyProbe *probe;
  GValueArray *va;
  gchar *prop_name;
  GList *list = NULL;
  guint i = 0;

  source = gst_element_factory_make (source_name, "source");
  if (!source || !GST_IS_PROPERTY_PROBE (source))
    goto finish;
  gst_element_set_state (source, GST_STATE_READY);
  gst_element_get_state (source, NULL, NULL, 5 * GST_SECOND);
  probe = GST_PROPERTY_PROBE (source);

  if (!g_strcmp0 (source_name, "dv1394src"))
    prop_name = "guid";
  else if (!g_strcmp0 (source_name, "v4l2src") ||
      !g_strcmp0 (source_name, "avfvideosrc"))
    prop_name = "device";
  else if (!g_strcmp0 (source_name, "decklinkvideosrc"))
    prop_name = "device-number";
  else if (!g_strcmp0 (source_name, "filesrc"))
    prop_name = "location";
  else
    prop_name = "device-name";

  va = gst_property_probe_probe_and_get_values_name (probe, prop_name);
  gst_element_set_state (source, GST_STATE_NULL);
  gst_element_get_state (source, NULL, NULL, 5 * GST_SECOND);
  gst_object_unref (source);

  if (!va)
    goto finish;


  for (i = 0; i < va->n_values; ++i) {
    GValue *v = g_value_array_get_nth (va, i);
    GValue valstr = { 0, };
    LgmDevice *device;
    gchar *name;

    g_value_init (&valstr, G_TYPE_STRING);
    if (!g_value_transform (v, &valstr))
      continue;

    /* Skip blackmagic on avfvideosrc as we only properly support them
     * through decklinkvideosrc. */
    if (!g_strcmp0 (source_name, "avfvideosrc"))
      if (!g_strcmp0 (g_value_get_string (&valstr), "Blackmagic"))
        continue;

    /* Use the pattern "Blackmagic%d" for device name when decklinkvideosrc */
    if (!g_strcmp0 (source_name, "decklinkvideosrc"))
      name = g_strdup_printf ("Blackmagic%s", g_value_get_string (&valstr));
    else
      name = g_value_dup_string (&valstr);
    device = lgm_device_new (source_name, name, type);
    g_value_unset (&valstr);
    g_free (name);

    lgm_device_fill_formats (device, prop_name);
    list = g_list_append (list, device);
  }
  g_value_array_free (va);

finish:
  {
    return list;
  }
}

GList *
lgm_device_enum_video_devices (const gchar * device)
{
  return lgm_device_enum_devices (device, LGM_DEVICE_TYPE_VIDEO);
}

GList *
lgm_device_enum_audio_devices (const gchar * device)
{
  return lgm_device_enum_devices (device, LGM_DEVICE_TYPE_AUDIO);
}
