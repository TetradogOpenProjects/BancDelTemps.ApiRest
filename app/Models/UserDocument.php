<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class UserDocument extends Model
{
    use HasFactory;
    // turn off both 
    public $timestamps = false;
    protected $table="UserDocuments";
    public function File(){
        return $this->belongsTo(File::class);
    }
}
